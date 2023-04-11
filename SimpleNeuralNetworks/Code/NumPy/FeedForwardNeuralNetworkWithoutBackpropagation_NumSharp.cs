
namespace SimpleNeuralNetworks
{
    
    using NumSharp;
    
    public class FeedForwardNeuralNetworkWithoutBackpropagation_NumSharp
    {
        private int inputCount; // number of input neurons
        private int outputCount; // number of output neurons
        private int hiddenCount; // number of hidden neurons
        private NDArray inputWeights; // weights between input and hidden layer
        private NDArray hiddenWeights; // weights between hidden and output layer

        public FeedForwardNeuralNetworkWithoutBackpropagation_NumSharp(int inputCount, int outputCount, int hiddenCount)
        {
            this.inputCount = inputCount;
            this.outputCount = outputCount;
            this.hiddenCount = hiddenCount;

            // initialize weights between input and hidden layer
            inputWeights = np.full((inputCount, hiddenCount), 0.5);

            // initialize weights between hidden and output layer
            hiddenWeights = np.full((hiddenCount, outputCount), 0.5);
        }

        // forward pass of the neural network
        public NDArray Forward(NDArray input)
        {
            // compute hidden layer activations
            NDArray hidden = np.zeros(hiddenCount);
            for (int i = 0; i < hiddenCount; i++)
            {
                NDArray sum = np.sum(np.multiply(input, inputWeights.T[i]));
                hidden[i] = Sigmoid(sum);
            }

            // compute output layer activations
            NDArray output = np.zeros(outputCount);
            for (int i = 0; i < outputCount; i++)
            {
                NDArray sum = np.sum(np.multiply(hidden, hiddenWeights.T[i]));
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