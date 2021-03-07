namespace RuneOptim.BuildProcessing {
    /// <summary>
    /// Helpers for Permutation maths
    /// </summary>
    public static class PermutationsAndCombinations {
        public static long nCr(int n, int r) {
            // naive: return Factorial(n) / (Factorial(r) * Factorial(n - r));
            return nPr(n, r) / Factorial(r);
        }

        public static long nPr(int n, int r) {
            // naive: return Factorial(n) / Factorial(n - r);
            return FactorialDivision(n, n - r);
        }

        private static long FactorialDivision(int topFactorial, int divisorFactorial) {
            long result = 1;
            for (int i = topFactorial; i > divisorFactorial; i--)
                result *= i;
            return result;
        }

        private static long Factorial(int i) {
            if (i <= 1)
                return 1;
            return i * Factorial(i - 1);
        }
    }

}
