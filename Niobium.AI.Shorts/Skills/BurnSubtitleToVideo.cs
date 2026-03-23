using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Skills
{
    internal class BurnSubtitleToVideo
    {
        private const string EmphasisWordRegexFormat = "\\b({0})\\b";

        public static async Task<Stream> BurnInSubtitlesAsync(Stream videoStream, IVideoInstruction videoInstruction, SubtitlePlan subtitlePlan, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(videoStream);
            ArgumentNullException.ThrowIfNull(videoInstruction);
            ArgumentNullException.ThrowIfNull(subtitlePlan);

            if (videoInstruction.VideoWidth <= 0 || videoInstruction.VideoHeight <= 0)
            {
                throw new InvalidOperationException("Video dimensions must be set before burning subtitles.");
            }

            if (subtitlePlan.Captions is null || subtitlePlan.Captions.Count == 0)
            {
                return videoStream;
            }

            var tempVideoPath = Path.Combine(Path.GetTempPath(), $"video_{Guid.NewGuid():N}.mp4");
            try
            {
                using (videoStream)
                using (var fileStream = File.Create(tempVideoPath))
                {
                    await videoStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                }

                var outputPath = Path.Combine(Path.GetTempPath(), $"subs_{Guid.NewGuid():N}.mp4");
                var assPath = Path.Combine(Path.GetTempPath(), $"subs_{Guid.NewGuid():N}.ass");

                try
                {
                    await File.WriteAllTextAsync(assPath, BuildAss(subtitlePlan, videoInstruction.VideoWidth, videoInstruction.VideoHeight), Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                    await RunFFmpegBurnAsync(tempVideoPath, outputPath, assPath, cancellationToken).ConfigureAwait(false);
                    return File.OpenRead(outputPath);
                }
                catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
                {
                    throw new InvalidOperationException("Failed to start ffmpeg to burn subtitles.", ex);
                }
                finally
                {
                    try
                    {
                        File.Delete(assPath);
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                try
                {
                    File.Delete(tempVideoPath);
                }
                catch
                {
                }
            }
        }

        private static string BuildAss(SubtitlePlan plan, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(plan);

            var fontSize = Math.Max(10, plan.Style.FontSizePt > 0 ? plan.Style.FontSizePt : (int)Math.Round(height * 0.06));
            var outlinePx = Math.Max(1, plan.Style.OutlineWidthPt > 0 ? plan.Style.OutlineWidthPt : (int)Math.Round(fontSize * 0.08));
            var marginV = Math.Max(10, (int)Math.Round(height * 0.06));
            var marginLR = Math.Max(10, (int)Math.Round(width * 0.06));
            var primary = ColorToAssPrimary(plan.Style.ColorRgb);
            var outlineColor = ColorToAssPrimary(plan.Style.OutlineRgb);

            var header = new StringBuilder()
                .AppendLine("[Script Info]")
                .AppendLine("ScriptType: v4.00+")
                .AppendLine("WrapStyle: 2")
                .AppendLine("ScaledBorderAndShadow: yes")
                .AppendLine($"PlayResX: {width}")
                .AppendLine($"PlayResY: {height}")
                .AppendLine()
                .AppendLine("[V4+ Styles]")
                .AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding")
                .Append("Style: Default,Arial,")
                .Append(fontSize.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(primary).Append(',')
                .Append("&H00000000,")
                .Append(outlineColor).Append(',')
                .Append("&H00000000,0,0,0,0,100,100,0,0,1,")
                .Append(outlinePx.ToString(CultureInfo.InvariantCulture)).Append(",0,2,")
                .Append(marginLR.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(marginLR.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(marginV.ToString(CultureInfo.InvariantCulture)).AppendLine(",1")
                .AppendLine()
                .AppendLine("[Events]")
                .AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");

            foreach (var caption in plan.Captions)
            {
                var textRaw = String.Join("\\N", caption.Text.Select(EscapeAssText));
                var text = ApplyEmphasis(textRaw, caption.Emphasis);
                _ = header.Append("Dialogue: 0,")
                    .Append(AssTime(caption.Start)).Append(',')
                    .Append(AssTime(caption.End)).Append(",Default,,0,0,0,,")
                    .AppendLine(text);
            }

            return header.ToString();
        }

        private static string AssTime(double seconds)
        {
            var totalCs = (int)Math.Round(seconds * 100, MidpointRounding.AwayFromZero);
            var cs = totalCs % 100;
            var totalS = totalCs / 100;
            var s = totalS % 60;
            var totalM = totalS / 60;
            var m = totalM % 60;
            var h = totalM / 60;
            return String.Create(CultureInfo.InvariantCulture, $"{h}:{m:00}:{s:00}.{cs:00}");
        }

        private static string EscapeAssText(string text) =>
            (text ?? String.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Replace("{", "\\{", StringComparison.Ordinal)
                .Replace("}", "\\}", StringComparison.Ordinal)
                .Replace("\n", "\\N", StringComparison.Ordinal);

        private static string ApplyEmphasis(string text, string? emphasisWord)
        {
            if (String.IsNullOrWhiteSpace(emphasisWord))
            {
                return text;
            }

            var regex = new Regex(String.Format(CultureInfo.InvariantCulture, EmphasisWordRegexFormat, Regex.Escape(emphasisWord)), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            return regex.Replace(text, "{\\b1}$1{\\b0}", 1);
        }

        private static string ColorToAssPrimary(string? color)
        {
            if (String.IsNullOrWhiteSpace(color))
            {
                return "&H00FFFFFF";
            }

            var normalized = color.Trim();
            if (String.Equals(normalized, "white", StringComparison.OrdinalIgnoreCase))
            {
                return "&H00FFFFFF";
            }

            if (String.Equals(normalized, "black", StringComparison.OrdinalIgnoreCase))
            {
                return "&H00000000";
            }

            var hex = normalized.TrimStart('#');
            return hex.Length == 6 && Int32.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
                ? String.Create(CultureInfo.InvariantCulture, $"&H00{hex[4..6]}{hex[2..4]}{hex[0..2]}").ToUpperInvariant()
                : "&H00FFFFFF";
        }

        private static async Task RunFFmpegBurnAsync(string inputPath, string outputPath, string assPath, CancellationToken cancellationToken)
        {
            var escapedAssPath = assPath.Replace("\\", "/", StringComparison.Ordinal).Replace(":", "\\:", StringComparison.Ordinal);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            processStartInfo.ArgumentList.Add("-y");
            processStartInfo.ArgumentList.Add("-i");
            processStartInfo.ArgumentList.Add(inputPath);
            processStartInfo.ArgumentList.Add("-vf");
            processStartInfo.ArgumentList.Add($"ass='{escapedAssPath}'");
            processStartInfo.ArgumentList.Add("-c:v");
            processStartInfo.ArgumentList.Add("libx264");
            processStartInfo.ArgumentList.Add("-crf");
            processStartInfo.ArgumentList.Add("18");
            processStartInfo.ArgumentList.Add("-preset");
            processStartInfo.ArgumentList.Add("medium");
            processStartInfo.ArgumentList.Add("-c:a");
            processStartInfo.ArgumentList.Add("copy");
            processStartInfo.ArgumentList.Add(outputPath);

            using var process = new Process { StartInfo = processStartInfo };
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start ffmpeg process.");
            }

            var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var standardOutput = await standardOutputTask.ConfigureAwait(false);
            var standardError = await standardErrorTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"ffmpeg failed with code {process.ExitCode}: {standardError}{standardOutput}");
            }
        }
    }
}
