# EntityDb.NET [![Codacy Badge](https://app.codacy.com/project/badge/Coverage/d9c2b2e4e1ba42918ffeb2377d35bfab)](https://www.codacy.com/gh/entitydb-io/EntityDb.NET/dashboard?utm_source=github.com&utm_medium=referral&utm_content=entitydb-io/EntityDb.NET&utm_campaign=Badge_Coverage) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/d9c2b2e4e1ba42918ffeb2377d35bfab)](https://www.codacy.com/gh/entitydb-io/EntityDb.NET/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=entitydb-io/EntityDb.NET&amp;utm_campaign=Badge_Grade) [![Build](https://github.com/entitydb-io/EntityDb.NET/actions/workflows/build.yml/badge.svg)](https://github.com/entitydb-io/EntityDb.NET/actions/workflows/build.yml)

At its core, EntityDb.NET is a set of abstractions and implementations for the Event Sourcing pattern, with the added
ability to enforce uniqueness constraints and the ability to tag resources.

## What is Event Sourcing?

Event Sourcing centers around the idea that your source of truth is _not_ the current state, it is all of the deltas
that add up to the current state. Consider your personal bank - which of these options do you think it their source of
truth for account balances?

- Option A - Table Rows
    - John Doe has $123.45 in Account A
    - Jane Doe has $678.90 in Account B
- Option B - Transactions
    - John Doe deposited $100.00 in Account A
    - Jane Doe deposited $600.00 in Account B
    - John Doe deposited $20.00 in Account A
    - Jane Doe deposited $70.00 in Account B
    - John Doe deposited $3.45 in Account A
    - Jane Doe deposited $8.90 in Account B

The answer is pretty obvious if you go check your bank statement. They keep a set of transactions, and regurgitate that
information on the statement. (If I'm wrong, you should consider getting a new bank, ASAP!)

## How does EntityDb.NET implement Event Sourcing?

There are five core objects at the heart of this implementation

1. Transactions
2. Agent
3. Commands
4. Tags
5. Leases

### Transactions

A transaction represents an atomic operation on multiple entities. A transaction is ideally* committed atomically or not at all.
If some step in the transaction fails, the entire transaction should fail.

*In the MongoDb implementation, the transaction _is_ committed atomically. However, it is possible in the future that there
will be implementations that are not capable of doing this if you want to use tags and/or leases. An example would be EventStore,
which provides no ability to enforce uniqueness constraints in its transaction. Such implementations will need a complimentary
transaction in order to make use of tags and leases.

### Agent

An agent is an actor that can execute transactions. For example, if a transaction is initiated via an HTTP API, you might use
the `HttpContextAgent` - it's signature includes headers and connection information, and it uses the ClaimsPrincipal to decide
if an agent has a particular role required for authorized commands.

### Commands

A command represents the intent to perform some operation on a single entity. Going back to the bank account example, one
command could be `PerformDeposit` while another could be `PerformWithdrawl`. The things that you can do are commands.

### Tags

A tag is a way to index entities by some piece of information. A tag can have a label and a value, both of which are strings.
Many accounts are typed, and you could represent this with a tag where `Label` is `Type` and `Value` is `Savings` or `Checking`.
You could then run a query to get the account id of all accounts where `Label` is `Type` and `Value` is `Savings`. The number
of savings accounts in the system would be the number of entity ids.

### Leases

A lease is like a tag, except that it has a uniqueness constraint. Many banks have online portals, allowing bank members
to see their accounts on the internet. From the bank's perspective, all of the accounts should be tied to a member id,
probably a guid. But the member will not want to remember nor lookup this guid - they will want to use a username.
What you can do in EntityDb is make a lease for member entities where the entity id is the member id, the `Label` is `Username`
and the `Value` is whatever username the member wants to use. If an attempt to commit a transaction is made that would violate the
uniqueness constraint, it will be rejected. (This is obnoxious behavior for the user, though, so the bank should check before attempting
to commit to see if the username is available and give immediate feedback to choose a different username).
