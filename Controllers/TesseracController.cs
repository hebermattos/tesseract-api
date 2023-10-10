using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace tesseract_test.Controllers;

[ApiController]
[Route("[controller]")]
public class TesseracController : ControllerBase
{

    [HttpPost]
    public List<OcrResult> Post(IFormFile file)
    {
        var notValidPageSegModes = new List<string>() { "Count", "OsdOnly" };

        var engineModes = Enum.GetNames(typeof(EngineMode)).ToList();
        var pageSegModes = Enum.GetNames(typeof(PageSegMode)).Where(x => !notValidPageSegModes.Contains(x)).ToList();

        var result = new List<OcrResult>();

        foreach (var engineMode in engineModes)
        {
            using var eng = new TesseractEngine(@"./tessdata", "eng", (EngineMode)Enum.Parse(typeof(EngineMode), engineMode, true));

            foreach (var pageSegMode in pageSegModes)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    file.CopyTo(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    using var img = Pix.LoadFromMemory(fileBytes);            
                    using var page = eng.Process(img,(PageSegMode)Enum.Parse(typeof(PageSegMode), pageSegMode, true));
                    var text = page.GetText();
                    var meanConfidence = page.GetMeanConfidence();

                    result.Add(new OcrResult
                    {
                        Text = text,
                        EngineMode = engineMode,
                        PageSegMode = pageSegMode,
                        MeanConfidence = meanConfidence
                    });
                }
                catch (Exception ex)
                {
                    result.Add(new OcrResult
                    {
                        Text = ex.ToString(),
                        EngineMode = engineMode,
                        PageSegMode = pageSegMode,
                        MeanConfidence = 0
                    });
                }
            }
        }

        return result
            .OrderByDescending(x => x.MeanConfidence)
            .ToList();
    }
}