using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageSprites;

namespace ImageSpritesVsix
{
    class SpriteService
    {
        private static SpriteGenerator _generator;

        public static void Initialize()
        {
            SpriteDocument.Saving += SpriteSaving;
            SpriteDocument.Saved += SpriteSaved;

            _generator = new SpriteGenerator();
            _generator.Saving += SpriteImageSaving;
            _generator.Saved += SpriteImageSaved;
        }

        private static void SpriteImageSaved(object sender, SpriteImageGenerationEventArgs e)
        {
            ProjectHelpers.AddNestedFile(e.Document.FileName, e.FileName);
            string ext = Path.GetExtension(e.FileName);

            if (e.Document.Optimize != Optimizations.None && Constants.SupporedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                ProjectHelpers.SelectInSolutionExplorer(e.FileName);
                string cmd = "ProjectandSolutionContextMenus.Project.ImageOptimizer.OptimzeImagelossless";

                if (e.Document.Optimize == Optimizations.Lossy)
                    cmd = "ProjectandSolutionContextMenus.Project.ImageOptimizer.OptimzeImagelossy";

                ProjectHelpers.ExecuteCommand(cmd);
            }
        }

        private static void SpriteImageSaving(object sender, SpriteImageGenerationEventArgs e)
        {
            ProjectHelpers.CheckFileOutOfSourceControl(e.FileName);
        }

        private static void SpriteSaved(object sender, FileSystemEventArgs e)
        {
            var project = ProjectHelpers.GetActiveProject();

            if (project != null)
            {
                ProjectHelpers.AddFileToProject(project, e.FullPath, "None");
            }
        }

        private static void SpriteSaving(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                ProjectHelpers.CheckFileOutOfSourceControl(e.FullPath);
            }
        }

        public static async Task GenerateSprite(string fileName)
        {
            var doc = await SpriteDocument.FromFile(fileName);
            await GenerateSprite(doc);
        }

        public static async Task GenerateSprite(SpriteDocument doc)
        {
            await _generator.Generate(doc);
        }
    }
}
