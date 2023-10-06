using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace tesseract_test.Controllers;

[ApiController]
[Route("[controller]")]
public class TesseracController : ControllerBase
{
    private TesseractEngine _tesseractEngine;

    public TesseracController(TesseractEngine tesseractEngine)
    {
        _tesseractEngine = tesseractEngine;
    }

    [HttpPost]
    public string Post(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            var fileBytes = memoryStream.ToArray();

            using (var img = Pix.LoadFromMemory(fileBytes))
            {
                using (var page = _tesseractEngine.Process(img))
                {
                    var text = page.GetText();

                    return text;
                }
            }
        }
    }
}
