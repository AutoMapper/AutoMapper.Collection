using System.Collections.Generic;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection
{
    public class NotExistingTests
    {
        [Fact]
        public void Should_not_throw_exception_for_nonexisting_types()
        {
            var configuration = new MapperConfiguration(x =>
            {
                //x.CreateMissingTypeMaps = false;
                x.AddCollectionMappers();
            });
            IMapper mapper = new Mapper(configuration);

            var system = new System
            {
                Name = "My First System",
                Contacts = new List<Contact>
                {
                    new Contact
                    {
                        Name = "John",
                        Emails = new List<Email>()
                        {
                            new Email
                            {
                                 Address = "john@doe.com"
                            }
                        }
                    }
                }
            };

            mapper.Map<SystemViewModel>(system);
        }

        public class System
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<Contact> Contacts { get; set; }
        }

        public class Contact
        {
            public int Id { get; set; }
            public int SystemId { get; set; }
            public string Name { get; set; }

            public System System { get; set; }

            public ICollection<Email> Emails { get; set; }
        }

        public class Email
        {
            public int Id { get; set; }

            public int ContactId { get; set; }
            public string Address { get; set; }

            public Contact Contact { get; set; }
        }

        public class SystemViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<ContactViewModel> Contacts { get; set; }
        }

        public class ContactViewModel
        {
            public int Id { get; set; }
            public int SystemId { get; set; }
            public string Name { get; set; }

            public SystemViewModel System { get; set; }

            public ICollection<EmailViewModel> Emails { get; set; }
        }

        public class EmailViewModel
        {
            public int Id { get; set; }
            public int ContactId { get; set; }
            public string Address { get; set; }

            public ContactViewModel Contact { get; set; }
        }
    }
}
