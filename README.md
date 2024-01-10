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

There are several core objects at the heart of this implementation. Encapsulating these objects are various
repositories.

1. Source Repository
    - Agents
    - Deltas
    - Tags
    - Leases
    - Aliases
2. Snapshot Repository
    - Snapshots
3. Entity Repository
    - Transaction Repository
    - Optional: Snapshot Repository
4. Projection Repository
    - Transaction Repository
    - Optional: Snapshot Repository

### Transactions

A source represents an atomic operation on multiple entities. A source is committed atomically or not
at all. If some step in the source fails, the entire source should fail.

### Agents

An agent is an actor that can record sources. For example, if a source is initiated via an HTTP API, you
might use the `HttpContextAgent` - it's signature includes headers and connection information.

### Delta

A delta represents a change to a single entity. Going back to the bank account example,
one delta could be `PerformDeposit` while another could be `PerformWithdrawl`. The things that you can do (commands),
as well as things that are done elsewhere (events), are delta.

### Tags

A tag is a way to index entities by some piece of information. A tag can have a label and a value, both of which are
strings. Many accounts are typed, and you could represent this with a tag where `Label` is `Type` and `Value`
is `Savings` or `Checking`. You could then run a query to get the account id of all accounts where `Label` is `Type`
and `Value` is `Savings`. The number of savings accounts in the system would be the number of entity ids.

### Leases

A lease is like a tag, except that it has a uniqueness constraint. Many banks have online portals, allowing bank members
to see their accounts on the internet. From the bank's perspective, all of the accounts should be tied to a member id,
probably a guid. But the member will not want to remember nor lookup this id - they will want to use a username. What
you can do in EntityDb is make a lease for members where the `Scope` is `Global`, the `Label`
is `Username`, and the `Value` is whatever username the member wants to use. If an attempt to commit a source is made
that would
violate the uniqueness constraint, it will be rejected. (This is obnoxious behavior for the user, though, so the bank
should check before attempting to commit to see if the username is available and give immediate feedback to choose a
different username).

### Snapshots

A snapshot is a stateful object at a given point in time. They always have an identifier and a version.
Together, the identifier and version called a pointer. You can request different versions of a given snapshot
by using different pointers!

In the context of snapshots, the reserved version is reserved for pointing to the latest snapshot.
So if you want the latest version, you use a pointer with the exact id and the reserved version.
If you want a specific version, you can create pointer with the exact id and version you want.

The balance on your bank account is a snapshot. You can build that snapshot by summing all of the deposits and
withdrawls on your account. If you look at the bank statements, you will most likely see the snapshot of each bank
account for that statement, along with all of the deposits, withdrawls, and interest.

### Entities

An entity is conceptually an aggregate id inside of a bounded context, and it extends the concept of a snapshot.
In the banking example, there are multiple entities. You have a membership at the bank. That's an entity. You probably
have a checking account. That's an entity. And you might even have a savings account. That is also an entity!

Which bounded contexts these entiies live in is up to the business.

### Projections

A projection is an aggregate, but notably _not_ the aggregate id, and it too extends the concept of a snapshot.
In the banking example, one example of a projection could be your entire account balance. It can be anything, though!
You are not constrained in what data you want to use for your projection.
