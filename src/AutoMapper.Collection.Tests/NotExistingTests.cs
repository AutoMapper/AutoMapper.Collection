using System;
using System.Collections.Generic;
using System.Linq;
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
                x.AddCollectionMappers();
            });
            IMapper mapper = new Mapper(configuration);

            var originalModel = new System
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

            var originalEmail = originalModel.Contacts.Single().Emails.Single();

            var assertModel = mapper.Map<SystemViewModel>(originalModel);
            assertModel.Name.Should().Be(originalModel.Name);
            assertModel.Contacts.Single().Name.Should().Be(originalModel.Contacts.Single().Name);
            assertModel.Contacts.Single().Emails.Single().Address.Should().Be(originalModel.Contacts.Single().Emails.Single().Address);

            assertModel.Contacts.Single().Emails.Add(new EmailViewModel { Address = "jane@doe.com" });

            mapper.Map(assertModel, originalModel);
        }

        public class System
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; }

            public ICollection<Contact> Contacts { get; set; }
        }

        public class Contact
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public Guid SystemId { get; set; }
            public string Name { get; set; }

            public System System { get; set; }

            public ICollection<Email> Emails { get; set; }
        }

        public class Email
        {
            public Guid Id { get; set; } = Guid.NewGuid();

            public Guid ContactId { get; set; }
            public string Address { get; set; }

            public Contact Contact { get; set; }
        }

        public class SystemViewModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public ICollection<ContactViewModel> Contacts { get; set; }
        }

        public class ContactViewModel
        {
            public Guid Id { get; set; }
            public Guid SystemId { get; set; }
            public string Name { get; set; }

            public SystemViewModel System { get; set; }

            public ICollection<EmailViewModel> Emails { get; set; }
        }

        public class EmailViewModel
        {
            public Guid Id { get; set; }
            public Guid ContactId { get; set; }
            public string Address { get; set; }

            public ContactViewModel Contact { get; set; }
        }
    }
}
