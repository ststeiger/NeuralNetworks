namespace SimpleNeuralNetworks
{
    using System;
    using NumSharp;

 
    public class ExtremelySimpleFeedForwardNeuralNetwork
    {
        private int inputSize;
        private int outputSize;
        private int hiddenSize;
        private NDArray weights;

        public ExtremelySimpleFeedForwardNeuralNetwork(int inputSize, int outputSize, int hiddenSize)
        {
            this.inputSize = inputSize;
            this.outputSize = outputSize;
            this.hiddenSize = hiddenSize;

            // Initialize weights randomly
            var rand = new Random();
            weights = np.random.randn(inputSize, hiddenSize);
        }

        public NDArray Predict(NDArray input)
        {
            // Multiply input by weights to get output
            var output = np.dot(input, weights);
            return output;
        }
        
        
    }
}