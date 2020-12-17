using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionWithLinq
{
    class Program
    {
        static readonly IReadOnlyList<Job> jobs = new List<Job>
        {
            new Job { JobNumber = "J00001", Description = "Fix the widget", AssignedTo = "Bob", Status = Status.Received },
            new Job { JobNumber = "J00002", Description = "Fix the doodad", AssignedTo = "Bob", Status = Status.Paid },
            new Job { JobNumber = "J00003", Description = "Fix the doohickey", AssignedTo = "Mary", Status = Status.Paid},
            new Job { JobNumber = "J00004", Description = "Fix the gadget", AssignedTo = "Sue", Status = Status.Complete },
            new Job { JobNumber = "J00005", Description = "Build the gizmo", AssignedTo = "John", Status = Status.Received },
            new Job { JobNumber = "J00006", Description = "Build the thingamabob", AssignedTo = "Mary", Status = Status.Paid },
            new Job { JobNumber = "J00007", Description = "Build the appliance", AssignedTo = "Sue", Status = Status.Processing },
            new Job { JobNumber = "J00008", Description = "Clean the device", AssignedTo = "Bob", Status = Status.Received },
            new Job { JobNumber = "J00009", Description = "Clean the wingbat", AssignedTo = "John", Status = Status.Paid },
            new Job { JobNumber = "J00010", Description = "Clean the grabber", AssignedTo = "Sue", Status = Status.Paid },
            new Job { JobNumber = "J00011", Description = "Clean the whatchamacallit", AssignedTo = "Mary", Status = Status.Cancelled},
            new Job { JobNumber = "J00012", Description = "Clean the whatsit", AssignedTo = "Jane", Status = Status.Complete },
        }.AsReadOnly();

        static void Main(string[] args)
        {
            Console.WriteLine("Jobs assigned to Mary:" + Environment.NewLine);
            foreach (var job in GetJobsByPerson("Mary"))
            {
                Console.WriteLine("  " + job.Description);
            }

            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Jobs containing the phrase 'the w':" + Environment.NewLine);
            foreach (var job in GetJobsByDescription("the w"))
            {
                Console.WriteLine("  " + job.Description);
            }

            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Jobs assigned to Mary containing the phrase 'the w':" + Environment.NewLine);
            foreach (var job in GetJobsByPersonAndDescription("Mary", "the w"))
            {
                Console.WriteLine("  " + job.Description);
            }

            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Jobs assigned to Bob, Mary or Sue:" + Environment.NewLine);
            foreach (var job in GetJobsByPerson("Bob", "Mary", "Sue"))
            {
                Console.WriteLine("  " + job.Description);
            }

            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Jobs assigned to Bob, Mary or Sue (using an Expression):" + Environment.NewLine);
            foreach (var job in GetJobsByPersonWithExpression("Bob", "Mary", "Sue"))
            {
                Console.WriteLine("  " + job.Description);
            }

            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Jobs assigned to Bob, and Mary's cancelled jobs:" + Environment.NewLine);
            foreach (var job in GetJobs(
                new JobSearchCriteria { AssignedTo = "Bob" },
                new JobSearchCriteria { AssignedTo = "Mary", Status = Status.Cancelled} ))
            {
                Console.WriteLine("  " + job.Description);
            }
        }

        static IEnumerable<Job> GetJobsByPerson(string person)
        {
            return jobs.Where(j => j.AssignedTo == person);
        }

        static IEnumerable<Job> GetJobsByPerson(params string[] people)
        {
            return jobs.Where(j => people.Contains(j.AssignedTo));
        }

        static IEnumerable<Job> GetJobsByPersonWithExpression(params string[] people)
        {
            if (people == null || people.Length == 0)
            {
                // If no list of people is included, return an empty list
                return new List<Job>().AsEnumerable();
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(Job));
            Expression peopleExpression = null;
            foreach (var person in people)
            {
                // Build an expression to check for this person
                Expression left = Expression.Property(parameterExpression, "AssignedTo");
                Expression right = Expression.Constant(person);
                Expression equals = Expression.Equal(left, right);

                // Combine the expression for this person with the expression for all the other
                // people, using a logical Or
                if (peopleExpression == null)
                {
                    peopleExpression = equals;
                }
                else
                {
                    peopleExpression = Expression.OrElse(peopleExpression, equals);
                }
            }

            // Now use the expression we've built to return the results we need
            var lambda = (Expression<Func<Job, bool>>)Expression.Lambda(peopleExpression, parameterExpression);
            return jobs.AsQueryable().Where(lambda);
        }

        static IEnumerable<Job> GetJobsByDescription(string search)
        {
            return jobs.Where(j => j.Description.Contains(search));
        }

        static IEnumerable<Job> GetJobsByPersonAndDescription(string person, string descriptionSearch)
        {
            IEnumerable<Job> result = jobs;

            if (person != null)
            {
                result = result.Where(j => j.AssignedTo == person);
            }

            if (descriptionSearch != null)
            {
                result = result.Where(j => j.Description.Contains(descriptionSearch));
            }

            return result;
        }

        static IEnumerable<Job> GetJobs(params JobSearchCriteria[] criteria)
        {
            if (criteria == null || criteria.Length == 0)
            {
                // If no list of people is included, return an empty list
                return new List<Job>().AsEnumerable();
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(Job));
            Expression peopleExpression = null;
            foreach (var criterion in criteria)
            {
                // Build an expression to check for this person
                Expression personLeft = Expression.Property(parameterExpression, "AssignedTo");
                Expression personRight = Expression.Constant(criterion.AssignedTo);
                Expression personEquals = Expression.Equal(personLeft, personRight);

                Expression match;

                if (criterion.Status == null)
                {
                    match = personEquals;
                }
                else
                {
                    // Build an expression to check for this status
                    Expression statusLeft = Expression.Property(parameterExpression, "Status");
                    Expression statusRight = Expression.Constant(criterion.Status);
                    Expression statusEquals = Expression.Equal(statusLeft, statusRight);

                    // And combine the two together using a local And
                    match = Expression.AndAlso(personEquals, statusEquals);
                }

                // Combine the expression for this person with the expression for all the other
                // people, using a logical Or
                if (peopleExpression == null)
                {
                    peopleExpression = match;
                }
                else
                {
                    peopleExpression = Expression.OrElse(peopleExpression, match);
                }
            }

            // Now use the expression we've built to return the results we need
            var lambda = (Expression<Func<Job, bool>>)Expression.Lambda(peopleExpression, parameterExpression);
            return jobs.AsQueryable().Where(lambda);
        }
    }
}
