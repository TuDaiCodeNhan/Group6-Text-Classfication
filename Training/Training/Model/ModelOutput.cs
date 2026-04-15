using Microsoft.ML.Data;

namespace ToxicCommentClassifier.Models;

public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}