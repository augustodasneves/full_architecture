using System;

namespace SupportAgent.Domain.Entities
{
    public class UserAccount
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        // PII Data
        public string PhoneNumber { get; private set; }
        public string Address { get; private set; }

        public UserAccount(string name, string email)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
        }

        // Constructor for EF Core
        protected UserAccount() { }

        public void UpdatePii(string phoneNumber, string address)
        {
            PhoneNumber = phoneNumber;
            Address = address;
        }
    }
}
