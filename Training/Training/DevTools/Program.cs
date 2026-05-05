using System.Text;
using ToxicCommentClassifier.DataPreprocessing;
using ToxicCommentClassifier.Logging;

static string WsPath(params string[] parts) => Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(parts));

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

Console.WriteLine("=== DevTools quick smoke checks ===");

// 1) DataCleaner: Vietnamese diacritics + links/mentions + repeated chars
var raw = "  @admin  Đồ án này làm chán quá!!! https://example.com  vãiiiiiii   ";
var cleaned = DataCleaner.CleanText(raw);
Console.WriteLine($"CleanText raw   : {raw}");
Console.WriteLine($"CleanText cleaned: {cleaned}");

// 2) DatasetProcessor: CSV with comma + quotes should not break
var inputCsv = WsPath("DevTools", "artifacts", "mini_input.csv");
var cleanedCsv = WsPath("DevTools", "artifacts", "mini_cleaned.csv");
Directory.CreateDirectory(Path.GetDirectoryName(inputCsv)!);

File.WriteAllText(inputCsv,
    "Message,IsToxic\n" +
    "\"Câu có dấu phẩy, và có \\\"ngoặc kép\\\"\",1\n" +
    "\"Bình thường thôi\",0\n" +
    "\"   \",0\n",
    new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

var stats = new DatasetProcessor().ProcessCsv(inputCsv, cleanedCsv);
Console.WriteLine($"DatasetProcessor stats: Total={stats.TotalRows}, Removed={stats.RemovedRows}, Toxic={stats.ToxicCount}, Safe={stats.SafeCount}");
Console.WriteLine($"Wrote: {cleanedCsv}");

// 3) DataAugmentor: should keep labels and increase dataset size
var augmentedCsv = WsPath("DevTools", "artifacts", "mini_augmented.csv");
var added = new DataAugmentor().AugmentCsv(cleanedCsv, augmentedCsv, minIncreaseRatio: 0.2);
Console.WriteLine($"DataAugmentor added rows: {added}. Wrote: {augmentedCsv}");

// 4) PredictionLogger: CSV escaping for comma/quotes
var logPath = WsPath("DevTools", "artifacts", "prediction_logs.csv");
PredictionLogger.Log("Câu có dấu phẩy, và có \"ngoặc kép\"", isToxic: true, confidence: 0.9123f, action: "Block & Review", logPath: logPath);
PredictionLogger.Log("Câu safe", isToxic: false, confidence: 0.1234f, action: "Allow", logPath: logPath);
Console.WriteLine($"PredictionLogger wrote: {logPath}");

// 5) LogAnalyzer: should not crash and should parse confidence
new LogAnalyzer().Analyze(logPath, lowConfidenceTop: 3);

Console.WriteLine("=== Done ===");

