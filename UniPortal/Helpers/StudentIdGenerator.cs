using UniPortal.Data;

namespace UniPortal.Helpers
{
    public class StudentIdGenerator
    {
        private readonly Random _random = new();

        // Generate a unique student ID given admission year and existing IDs
        public string GenerateStudentId(int admissionYear, IEnumerable<string> existingStudentIds)
        {
            string studentId;
            int maxAttempts = 10;
            int attempt = 0;

            var existingIdsSet = new HashSet<string>(existingStudentIds);

            do
            {
                attempt++;
                string sequentialNumber = GetNextSequentialNumber(admissionYear, existingIdsSet);
                string suffix = GenerateRandomSuffix(2);

                studentId = $"Y{admissionYear % 100:D2}{sequentialNumber}{suffix}";

            } while (existingIdsSet.Contains(studentId) && attempt < maxAttempts);

            if (attempt >= maxAttempts)
                throw new Exception("Unable to generate unique Student ID after multiple attempts.");

            return studentId;
        }

        private string GetNextSequentialNumber(int admissionYear, HashSet<string> existingIds)
        {
            int nextNumber = 1;

            var yearPrefix = $"Y{admissionYear % 100:D2}";

            var numbers = existingIds
                .Where(id => id.StartsWith(yearPrefix))
                .Select(id =>
                {
                    if (id.Length >= 6 && int.TryParse(id.Substring(3, 4), out var num))
                        return num;
                    return 0;
                })
                .ToList();

            if (numbers.Any())
                nextNumber = numbers.Max() + 1;

            return nextNumber.ToString("D4");
        }

        private string GenerateRandomSuffix(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[_random.Next(chars.Length)]).ToArray());
        }
    }

}
