﻿//Global Variable
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

/*
The Neocortex API generates sequences of numbers categorized as Even, Odd, or Neither, crucial for dataset creation. It learns from a file using 
LearnDatafromthefile, then splits the data 70-30 for training and testing using SplitData. The Classifier model is trained on 70% of the data to 
discern patterns, while 30% is reserved for performance evaluation. Testing employs the K-Nearest Neighbors Classifier, predicting labels using
Classifier method. Accuracy is assessed with CalculateAccuracy, comparing predicted and actual labels.
    

For an Example:

we have Sample Data in a Dataset which we split in training and testing data

training data = [
7665, 8260, 8304, 8495, 9285, 9366, 9388, 9603, 9641, 9707, 9774, 9819, 9837, 10020, 10096, 10149, 10263, 10313, 10873, 10914, 0
8360, 9729, 9769, 9826, 9860, 10039, 10064, 10169, 10338, 10408, 10461, 10609, 10669, 10689, 10792, 10889, 11235, 11339, 11435, 11672, 1
9460, 9558, 9725, 9883, 10336, 10393, 10896, 10933, 10982, 11113, 11173, 11423, 11719, 11835, 11897, 11902, 12075, 12164, 12415, 12715, 2]


testing Data = [8870, 9787, 9970, 10025, 10070, 10085, 10136, 10197, 10208, 10241, 10315, 10352, 10468, 10740, 10906, 11002, 11142, 11159, 11204, 11475, 1]


Here's the verdict: The model has predicted the testing data as Class 1, representing the odd number sequence SDR's.

The output includes the label class of the testing data and the accuracy of the model.

*/

namespace KNNImplementation
{
    /// <summary>
    /// KNN class is designed to perform classification tasks on sequences derived from the SDR (Sparse Distributed Representation) dataset
    /// </summary>

    public class KNNClassifier : IClassifier
    {
        /// <summary>
        /// Calculates the Euclidean distance between training Data and testing Data features.
        /// </summary>
        /// <param name="testData">The featuers of testing Data.</param>
        /// <param name="trainData">The featuers of testing Data.</param>
        /// <returns>The Euclidean distance between the two vectors.</returns>
        public double Distance(double[] testData, double[] trainData)
        {
            double sum = 0.0;

            for (int i = 0; i < testData.Length; ++i)
            {
                double difference = testData[i] - trainData[i];
                sum += difference * difference;
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Determines the class label by majority voting among the k nearest neighbors.
        /// </summary>
        /// <param name="info">An array of IndexAndDistance objects representing the k nearest neighbors.</param>
        /// <param name="trainData">The training data containing class labels.</param>
        /// <param name="numofclass">The total number of classes.</param>
        /// <param name="k">The number of nearest neighbors to consider.</param>
        /// <returns>The class label with the most votes among the nearest neighbors.</returns>

        static int Vote(IndexAndDistance[] info, double[][] trainData, int numofclass, int k)
        {
            int[] votes = new int[numofclass];

            for (int i = 0; i < numofclass; ++i)
            {
                votes[i] = 0;
            }

            for (int i = 0; i < k; ++i)
            {
                int idx = info[i].idx;
                int c = (int)trainData[idx][20];
                ++votes[c];
            }

            int mostVotes = 0;
            int classWithMostVotes = 0;

            // Loop through each class
            for (int j = 0; j < numofclass; ++j)
            {
                if (votes[j] > mostVotes)
                {
                 
                    mostVotes = votes[j];
                    classWithMostVotes = j;
                }
            }

            return classWithMostVotes;
        }

        /// <summary>
        /// Classifies the unknown SDR based on the k-nearest neighbors in the training data using the KNN algorithm.
        /// </summary>
        /// <param name="testData">The testing data containing unknown SDR to be classified.</param>
        /// <param name="trainData">The training data containing known SDRs.</param>
        /// <param name="numofclass">The total number of classes.</param>
        /// <param name="k">The number of nearest neighbors to consider in the classification.</param>
        /// <returns>The predicted class label for the unknown SDR.</returns>

        public  int Classifier(double[] testData, double[][] trainData, int numofclass, int k)
        {
            int n = trainData.Length;

            IndexAndDistance[] info = new IndexAndDistance[n];

            for (int i = 0; i < n; i++)
            {
                IndexAndDistance curr = new IndexAndDistance();

                double dist = Distance(testData, trainData[i]);

                curr.idx = i;
                curr.dist = dist;

                info[i] = curr;
            }

            Array.Sort(info);

            
            // Information for the k-nearest items is displayed
            Debug.WriteLine("   Nearest     /    Distance      /     Class   ");
            Debug.WriteLine("   ==========================================   ");
            for (int i = 0; i < k; ++i)
            {
                int c = (int)trainData[info[i].idx][20];
                string dist = info[i].dist.ToString("F3");
                Debug.WriteLine("( " + trainData[info[i].idx][0] +
                   "," + trainData[info[i].idx][1] + " )  :  " +
                  dist + "        " + c);
                
            }

            int result = Vote(info, trainData, numofclass, k);

            return result;
        }

        /// <summary>
        /// Creating Method to learn data from a datset file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns> Returning the dataset and storing it in Two Dimensional Array

        public double[][] LearnDatafromthefile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                double[][] datasets = new double[lines.Length][];

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');

                    datasets[i] = new double[values.Length];

                    for (int j = 0; j < values.Length; j++)
                    {
                        if (!double.TryParse(values[j], out datasets[i][j]))
                        {
                            throw new FormatException($"Failed to parse value at line {i + 1}, position {j + 1}");
                        }
                    }
                }

               
                return datasets;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during file reading or parsing
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Finding Accuracy of KNN CLassifue
        /// </summary>
        /// <param name="predictedLabels"></param>
        /// <param name="actualLabels"></param>
        /// <returns></returns> Retyrn the accuracy in Percentage 

        public  double CalculateAccuracy(int[] predictedLabels, int[] actualLabels)
        {
            int correctPredictions = 0;
            int totalPredictions = predictedLabels.Length;

            for (int i = 0; i < totalPredictions; i++)
            {
                if (predictedLabels[i] == actualLabels[i])
                {
                    correctPredictions++;
                }
            }

            double accuracy = (double)correctPredictions / totalPredictions * 100;
            return accuracy;
        }


        /// <summary>
        /// Method for Extracting AcutualLabels from test Dataset
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns> Returning the Acutual Labels from test data

        public int[] ExtractActualLabelsFromDataset(double[][] testData)
        {
            // Extract actual labels from the dataset
            // Assuming labels are stored in the last column of each row
            int[] actualLabels = new int[testData.Length];
            for (int i = 0; i < testData.Length; i++)
            {
                actualLabels[i] = (int)testData[i].Last();
            }
            return actualLabels;
        }


        /// <summary>
        /// Spliting the dataset in training dataset and testing dataset
        /// </summary>
        /// <param name="data"></param>
        /// <param name="trainRatio"></param>
        /// <returns></returns> Returning the split Training data and testing Data in Two dimensional Array 

        public (double[][], double[][]) SplitData(double[][] data, double trainRatio)
        {
            int totalRows = data.Length;
            int trainRows = (int)(totalRows * trainRatio);
            int testRows = totalRows - trainRows;

            int[] indices = Enumerable.Range(0, totalRows).ToArray();
            Shuffle(indices);

            double[][] trainData = new double[trainRows][];
            double[][] testData = new double[testRows][];

            for (int i = 0; i < totalRows; i++)
            {
                double[] row = data[indices[i]];
                if (i < trainRows)
                {
                    trainData[i] = row.ToArray();
                }
                else
                {
                    testData[i - trainRows] = row.ToArray();
                }
            }

            return (trainData, testData);
        }

        /// <summary>
        /// Method to shuffle an array of integers (Fisher-Yates shuffle algorithm)
        /// </summary>
        /// <param name="array"></param>
        static void Shuffle(int[] array)
        {
            Random rnd = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int index = rnd.Next(i + 1);
                int temp = array[index];
                array[index] = array[i];
                array[i] = temp;
            }
        }


    }


    /// <summary>
    /// Compares the instance to another based on distance.
    /// </summary>
    /// <param name="other">The other IndexAndDistance instance to compare with.</param>
    /// <returns>
    
    public class IndexAndDistance : IComparable<IndexAndDistance>
        {
            /// Represents an index and its distance to an unknown point in a dataset, used for sorting.
            /// Index of a training item.
            public int idx;

            /// Distance to the unknown point.
            public double dist;

            /// Compares this instance to another based on distance.
            /// A negative value if this instance has a smaller distance,
            /// a positive value if this instance has a larger distance,
            /// or zero if both instances have the same distance.
            public int CompareTo(IndexAndDistance other)
            {
                if (this.dist < other.dist)
                    return -1;
                else if (this.dist > other.dist)
                    return 1;
                else
                    return 0;
            }
     }


    
}