using System;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class Gene
    {
        public Allele allele;

        public Gene (Allele allele)
        {
            this.allele = allele;
        }
    }
}