using KNARZhelper;
using KNARZhelper.FilesCommon;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MetadataUtilities
{
    public class ImageProcessor
    {
        public static bool Mirror(Game game, FieldType fieldType, bool horizontally)
        {
            try
            {
                var imagePath = GetImagePath(game, fieldType);
                return !imagePath.IsNullOrEmpty() && ImageHelper.MirrorImage(imagePath, horizontally) && SaveChanges(game, fieldType, imagePath);
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Error mirroring {fieldType} for game {game.Name}: {ex.Message}");
                return false;
            }
        }

        public static bool Rotate(Game game, FieldType fieldType, int rotationAngle)
        {
            try
            {
                var imagePath = GetImagePath(game, fieldType);
                return !imagePath.IsNullOrEmpty() && ImageHelper.RotateImage(imagePath, rotationAngle) && SaveChanges(game, fieldType, imagePath);
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Error rotating {fieldType} for game {game.Name}: {ex.Message}");
                return false;
            }
        }

        private static string GetImagePath(Game game, FieldType fieldType)
        {
            if (game is null)
            {
                return null;
            }

            var typeManager = fieldType.GetTypeManager();

            if (typeManager is IImageType imageType)
            {
                var file = imageType.GetFile(game);

                return file.IsNullOrEmpty() || !File.Exists(file) ? null : file;
            }

            return null;
        }

        private static bool SaveChanges(Game game, FieldType fieldType, string imagePath)
        {
            try
            {
                if (!(fieldType.GetTypeManager() is BaseMediaType mediaType))
                {
                    return false;
                }

                if (fieldType != FieldType.Logo)
                {
                    var file = new FileInfo(imagePath);

                    var newName = Guid.NewGuid().ToString() + file.Extension;

                    file.Rename(newName);

                    mediaType.EmptyFieldInGame(game);

                    mediaType.SetValue(game, file.FullName);
                }

                ControlCenter.UpdateGames(new List<Game>() { game });

                return true;
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Error saving changes for {fieldType} for game {game.Name}: {ex.Message}");
                return false;
            }
        }
    }
}
