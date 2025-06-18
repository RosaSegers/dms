using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace FrontendEndToEnd
{
    [TestFixture]
    public class HomePageTest : PageTest
    {
        [Test]
        public async Task HomePageLoadsAndDisplaysFeatures()
        {
            // Navigate to the homepage
            await Page.GotoAsync("http://localhost:8080/");
            await Page.ScreenshotAsync(new() { Path = "homepage-error.png" });

            // Check main headline
            var heading = await Page.Locator("h2").TextContentAsync();
            Assert.That(heading, Does.Contain("Organize your documents"));

            // Check the "Get Started" button is visible
            var getStartedBtn = Page.Locator("text=🌸 Get Started");
            Assert.That(await getStartedBtn.IsVisibleAsync(), Is.True);

            // Check that each feature is visible
            var features = new[]
            {
                "📄 Easy Uploads",
                "🕒 Version Control",
                "🔐 Secure Sharing"
            };

            foreach (var feature in features)
            {
                var featureEl = Page.Locator($"text={feature}");
                Assert.That(await featureEl.IsVisibleAsync(), Is.True, $"Feature '{feature}' should be visible");
            }
        }
    }
}
