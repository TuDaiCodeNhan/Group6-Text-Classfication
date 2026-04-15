using Microsoft.ML.Data;

namespace ToxicCommentClassifier.Models;

public class ModelInput
{
    [LoadColumn(0)]
    public string TextContent { get; set; }

    [LoadColumn(1), ColumnName("Label")]
    public bool IsToxic { get; set; }
}