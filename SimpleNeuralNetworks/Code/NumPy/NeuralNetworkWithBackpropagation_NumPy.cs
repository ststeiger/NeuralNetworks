
namespace SimpleNeuralNetworks
{
    using NumSharp;


    public class NeuralNetworkWithBackpropagation_NumPy
    {
        private int inputCount; // number of input neurons
        private int outputCount; // number of output neurons
        private int hiddenCount; // number of hidden neurons
        private NDArray inputWeights; // weights between input and hidden layer
        private NDArray hiddenWeights; // weights between hidden and output layer
        private NDArray hiddenBias; // bias values for hidden layer
        private NDArray outputBias; // bias values for output layer

        public NeuralNetworkWithBackpropagation_NumPy(int inputCount, int outputCount, int hiddenCount)
        {
            this.inputCount = inputCount;
            this.outputCount = outputCount;
            this.hiddenCount = hiddenCount;

            // initialize weights between input and hidden layer
            inputWeights = np.random.uniform(-1, 1, (inputCount, hiddenCount));

            // initialize weights between hidden and output layer
            hiddenWeights = np.random.uniform(-1, 1, (hiddenCount, outputCount));

            // initialize bias values for hidden layer
            hiddenBias = np.ones(hiddenCount);

            // initialize bias values for output layer
            outputBias = np.ones(outputCount);
        }

        // train the neural network using backpropagation
        public void Train(double[] input, double[] targetOutput, double learningRate, int epochs)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                // forward pass
                NDArray hidden = np.zeros(hiddenCount);
                for (int i = 0; i < hiddenCount; i++)
                {
                    NDArray weights = inputWeights[$":,{i}"];
                    NDArray inputVector = np.array(input);
                    double sum = np.sum(inputVector * weights) + hiddenBias[i];
                    hidden[i] = Sigmoid(sum);
                }

                NDArray output = np.zeros(outputCount);
                for (int i = 0; i < outputCount; i++)
                {
                    NDArray weights = hiddenWeights[$":,{i}"];
                    double sum = np.sum(hidden * weights) + outputBias[i];
                    output[i] = Sigmoid(sum);
                }

                // backward pass
                NDArray outputError = np.array(targetOutput) - output;
                NDArray deltaOutput = output * (1 - output) * outputError;
                outputBias += learningRate * deltaOutput;
                hiddenWeights += learningRate * np.outer(hidden, deltaOutput);

                NDArray hiddenError = np.dot(hiddenWeights, deltaOutput.T).T;
                NDArray deltaHidden = hidden * (1 - hidden) * hiddenError;
                hiddenBias += learningRate * deltaHidden;
                inputWeights += learningRate * np.outer(np.array(input), deltaHidden);
            }
        }

        // compute output of the neural network for a given input
        public NDArray Compute(double[] input)
        {
            NDArray hidden = np.zeros(hiddenCount);
            for (int i = 0; i < hiddenCount; i++)
            {
                NDArray weights = inputWeights[$":,{i}"];
                NDArray inputVector = np.array(input);
                double sum = np.sum(inputVector * weights) + hiddenBias[i];
                hidden[i] = Sigmoid(sum);
            }

            NDArray output = np.zeros(outputCount);
            for (int i = 0; i < outputCount; i++)
            {
                NDArray weights = hiddenWeights[$":,{i}"];
                double sum = np.sum(hidden * weights) + outputBias[i];
                output[i] = Sigmoid(sum);
            }

            return output;
        }
        
        // sigmoid activation function
        private double Sigmoid(double x)
        {
            return 1 / (1 + System.Math.Exp(-x));
        }
        
        
    }
    
    
}
