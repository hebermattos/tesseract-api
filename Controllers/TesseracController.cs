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
        var engineModes = new List<EngineMode>(){
            EngineMode.Default,
            EngineMode.LstmOnly,
            EngineMode.TesseractAndLstm,
            EngineMode.TesseractOnly
        };

        var result = new List<OcrResult>();

        foreach (var item in engineModes)
        {
            using var eng = new TesseractEngine(@"./tessdata", "eng", item);

            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            var fileBytes = memoryStream.ToArray();

            using var img = Pix.LoadFromMemory(fileBytes);
            using var page = eng.Process(img);

            var text = page.GetText();
            var meanConfidence = page.GetMeanConfidence();

            result.Add(new OcrResult
            {
                Text = text,
                EngineMode = item.ToString(),
                MeanConfidence = meanConfidence
            });
        }

        return result;

    }
}