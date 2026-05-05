namespace ToxicCommentClassifier;
public static class AppConstants
{
    public static string DatasetPath => ResolveDatasetPath();

    public const string ModelSavePath = "TextClassifierModel.zip";

    private static string ResolveDatasetPath()
    {
        var candidates = new[]
        {
            "premium_toxic_dataset.csv",
            "Toxicdataset.csv",
            "toxic_dataset_1000.csv",
            Path.Combine("Data", "premium_toxic_dataset.csv"),
            Path.Combine("Data", "Toxicdataset.csv"),
            Path.Combine("Data", "toxic_dataset_1000.csv"),
        };

        var current = Directory.GetCurrentDirectory();
        foreach (var dir in EnumerateWithParents(current, maxDepth: 6))
        {
            foreach (var relative in candidates)
            {
                var path = Path.Combine(dir, relative);
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        throw new FileNotFoundException(
            "Không tìm thấy dataset CSV. Hãy đặt file dataset ở thư mục hiện tại hoặc các thư mục cha.",
            Path.Combine(current, candidates[0]));
    }

    private static IEnumerable<string> EnumerateWithParents(string start, int maxDepth)
    {
        var current = start;
        for (int i = 0; i <= maxDepth; i++)
        {
            yield return current;
            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrWhiteSpace(parent) || parent == current)
            {
                yield break;
            }
            current = parent;
        }
    }
}