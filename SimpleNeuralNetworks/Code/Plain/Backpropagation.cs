/*

To use this neural network, you can create an instance of the `NeuralNetwork` class 
with the desired number of input, output, and hidden neurons. 
Then, you can call the `Train` method with input and target output values, a learning rate, 
and the number of epochs to train for. 

Finally, you can call the `Compute` method with input values to get the output of the neural network.

*/

namespace SimpleNeuralNetworks
{


    public class NeuralNetworkWithBackpropagation
    {
        private int inputCount; // number of input neurons
        private int outputCount; // number of output neurons
        private int hiddenCount; // number of hidden neurons
        private double[,] inputWeights; // weights between input and hidden layer
        private double[,] hiddenWeights; // weights between hidden and output layer
        private double[] hiddenBias; // bias values for hidden layer
        private double[] outputBias; // bias values for output layer

        public NeuralNetworkWithBackpropagation(int inputCount, int outputCount, int hiddenCount)
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
                    inputWeights[i, j] = GetRandomWeight(inputCount);
                }
            }

            // initialize weights between hidden and output layer
            hiddenWeights = new double[hiddenCount, outputCount];
            for (int i = 0; i < hiddenCount; i++)
            {
                for (int j = 0; j < outputCount; j++)
                {
                    hiddenWeights[i, j] = GetRandomWeight(hiddenCount);
                }
            }

            // initialize bias values for hidden layer
            hiddenBias = new double[hiddenCount];
            for (int i = 0; i < hiddenCount; i++)
            {
                hiddenBias[i] = 1;
            }

            // initialize bias values for output layer
            outputBias = new double[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                outputBias[i] = 1;
            }
        }

        // train the neural network using backpropagation
        public void Train(double[] input, double[] targetOutput, double learningRate, int epochs)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                // forward pass
                double[] hidden = new double[hiddenCount];
                for (int i = 0; i < hiddenCount; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < inputCount; j++)
                    {
                        sum += input[j] * inputWeights[j, i];
                    }

                    sum += hiddenBias[i];
                    hidden[i] = Sigmoid(sum);
                }

                double[] output = new double[outputCount];
                for (int i = 0; i < outputCount; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < hiddenCount; j++)
                    {
                        sum += hidden[j] * hiddenWeights[j, i];
                    }

                    sum += outputBias[i];
                    output[i] = Sigmoid(sum);
                }

                // backward pass
                double[] outputError = new double[outputCount];
                for (int i = 0; i < outputCount; i++)
                {
                    outputError[i] = (targetOutput[i] - output[i]) * output[i] * (1 - output[i]);
                    outputBias[i] += learningRate * outputError[i];
                    for (int j = 0; j < hiddenCount; j++)
                    {
                        hiddenWeights[j, i] += learningRate * outputError[i] * hidden[j];
                    }
                }

                double[] hiddenError = new double[hiddenCount];
                for (int i = 0; i < hiddenCount; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < outputCount; j++)
                    {
                        sum += outputError[j] * hiddenWeights[i, j];
                    }

                    hiddenError[i] = sum * hidden[i] * (1 - hidden[i]);
                    hiddenBias[i] += learningRate * hiddenError[i];
                    for (int j = 0; j < inputCount; j++)
                    {
                        inputWeights[j, i] += learningRate * hiddenError[i] * input[j];
                    }
                }
            }
        }






        // compute output of the neural network for a given input
        public double[] Compute(double[] input)
        {
            double[] hidden = new double[hiddenCount];
            for (int i = 0; i < hiddenCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < inputCount; j++)
                {
                    sum += input[j] * inputWeights[j, i];
                }

                sum += hiddenBias[i];
                hidden[i] = Sigmoid(sum);
            }

            double[] output = new double[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < hiddenCount; j++)
                {
                    sum += hidden[j] * hiddenWeights[j, i];
                }

                sum += outputBias[i];
                output[i] = Sigmoid(sum);
            }

            return output;
        }

        // sigmoid activation function
        private double Sigmoid(double x)
        {
            return 1 / (1 + System.Math.Exp(-x));
        }

        // get a random weight value between -1 and 1
        private double GetRandomWeight(int count)
        {
            return 2.0 / count * (new System.Random().NextDouble() - 0.5);
        }


    }


}