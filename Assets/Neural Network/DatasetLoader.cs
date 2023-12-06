public interface DatasetLoader
{
    public struct DatasetItem
    {
        public float[] input;
        public float[] output;
    }

    public DatasetItem NextItem();

    public DatasetItem[] GetDataset();
}