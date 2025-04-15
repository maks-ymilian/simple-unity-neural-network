A simple interactive C# neural network implementation in Unity, with a visual of the neural network that displays the values of each node and weight as it trains

There are 2 scenes:
- `Function example` trains a network to reproduce a mathematical function.
- `Number example` trains on the MNIST dataset to recognize hand drawn digits.
In the number scene, you can draw your own digit and see how the model classifies it

You can tweak the neural network hyperparameters and see how it impacts training
- Layers: Number of neurons per layer
- Learning Rate: How fast the network updates weights during training
- Batch Size: Number of samples per training batch
- Weight Initialization Range: Min and max values for initial weights
- Bias Initialization Range: Min and max values for initial biases
- Activation Function: Sigmoid, ReLU, LeakyReLU

#Screenshots
![number recognition](https://raw.githubusercontent.com/maksymilllllllllllllian/simple-unity-neural-network/master/numbers.png)
![function learning](https://raw.githubusercontent.com/maksymilllllllllllllian/simple-unity-neural-network/master/function2.png)
![function learning](https://raw.githubusercontent.com/maksymilllllllllllllian/simple-unity-neural-network/master/function.png)
