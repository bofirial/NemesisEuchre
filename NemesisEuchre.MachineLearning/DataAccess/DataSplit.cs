using Microsoft.ML;

namespace NemesisEuchre.MachineLearning.DataAccess;

public record DataSplit(
    IDataView Train,
    IDataView Validation,
    IDataView Test,
    int TrainRowCount,
    int ValidationRowCount,
    int TestRowCount);
