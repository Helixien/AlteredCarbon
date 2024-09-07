using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Window_NeuralMatrixManagement : Window
    {
        public Building_NeuralMatrix matrix;
        public Window_NeuralMatrixManagement(Building_NeuralMatrix matrix)
        {
            this.matrix = matrix;
        }

        public override void DoWindowContents(Rect inRect)
        {

        }
    }
}