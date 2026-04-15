using System;
using Microsoft.ML;
using ToxicCommentClassifier.Models;

namespace ToxicCommentClassifier.ML
{
    public class Predictor
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        public Predictor()
        {
            _mlContext = new MLContext();

            // Load model từ file zip
            ITransformer trainedModel = _mlContext.Model.Load(AppConstants.ModelSavePath, out var modelInputSchema);

            // Tạo engine dự đoán
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);
        }

        public ModelOutput Predict(string textInput)
        {
            var input = new ModelInput { TextContent = textInput };
            return _predictionEngine.Predict(input);
        }
    }
}