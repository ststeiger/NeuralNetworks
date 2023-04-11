
namespace SimpleNeuralNetworks
{
    
    
    public class FeedForwardNeuralNetworkWithoutBackpropagation
    {
        private int inputCount; // number of input neurons
        private int outputCount; // number of output neurons
        private int hiddenCount; // number of hidden neurons
        private double[,] inputWeights; // weights between input and hidden layer
        private double[,] hiddenWeights; // weights between hidden and output layer

        public FeedForwardNeuralNetworkWithoutBackpropagation(int inputCount, int outputCount, int hiddenCount)
        {
            this.inputCount = inputCount;
            this.outputCount = outputCount;
            this.hiddenCount = hiddenCount;

            // initialize weights between input and hidden layer
            inputWeights = new double[inputCount, hiddenCount];
            for (int i = 0; i < inputCount; i++)
            {
                for (int j = 0; j < hiddenCount; j++)
                {
                    inputWeights[i, j] = 0.5; // set initial weight to 0.5
                }
            }

            // initialize weights between hidden and output layer
            hiddenWeights = new double[hiddenCount, outputCount];
            for (int i = 0; i < hiddenCount; i++)
            {
                for (int j = 0; j < outputCount; j++)
                {
                    hiddenWeights[i, j] = 0.5; // set initial weight to 0.5
                }
            }
        }

        // forward pass of the neural network
        public double[] Forward(double[] input)
        {
            // compute hidden layer activations
            double[] hidden = new double[hiddenCount];
            for (int i = 0; i < hiddenCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < inputCount; j++)
                {
                    sum += input[j] * inputWeights[j, i];
                }

                hidden[i] = Sigmoid(sum);
            }

            // compute output layer activations
            double[] output = new double[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < hiddenCount; j++)
                {
                    sum += hidden[j] * hiddenWeights[j, i];
                }

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