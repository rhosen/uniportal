namespace UniPortal.Helpers
{
    public class FacultyNumberGenerator
    {
        /// <summary>
        /// Generates the next Faculty Number in FYY-XXXX format.
        /// </summary>
        /// <param name="year">Full year, e.g., 2025</param>
        /// <param name="lastSequence">Last used sequence for this year</param>
        /// <returns>Next Faculty Number</returns>
        public string GenerateNext(int year, int lastSequence)
        {
            var yearSuffix = year % 100;           // last 2 digits of year, e.g., 25
            var prefix = $"F{yearSuffix:D2}";      // F25
            var nextSequence = lastSequence + 1;   // increment sequence
            return $"{prefix}-{nextSequence:D4}";  // e.g., F25-0001
        }
    }
}
