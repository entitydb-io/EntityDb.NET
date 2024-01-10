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
    - Agent Signatures
    - Deltas
    - Tags
    - Leases
2. State Repository
    - States
3. Stream Repository
4. Entity Repository
5. Projection Repository

### Sources

A source represents an atomic operation on one or more states.
If any message in the source cannot be committed, the entire source
cannot be committed.

### Agent Signatures

An agent is an actor that can record sources and interact with states. Whenever a source is committed,
the signature of the agent is recorded with it.

For example, if a source is initiated via an HTTP API, you
might use the `HttpContextAgent` - it's signature includes headers and connection information.

### Deltas

A delta represents a change in state. Literally anything can be a delta.

Going back to the bank account example,
one delta could be `PerformDeposit` while another could be `PerformWithdrawl`. The things that you can do (commands),
as well as things that are done elsewhere (events), are deltas.

### Tags

A tag is a way to index sources by some piece of information contained within. A tag can have a label and a value, both
of which are
strings.

Many accounts are typed, and you could represent this with a tag where `Label` is `Type` and `Value`
is `Savings` or `Checking`. You could then run a query to get the account id of all accounts where `Label` is `Type`
and `Value` is `Savings`. The number of savings accounts in the system would be the number of state ids.

### Leases

A lease is like a tag, except that it has a uniqueness constraint. In addition to a label and value, leases also have
a scope.

Many banks have online portals, allowing bank members
to see their accounts on the internet. From the bank's perspective, all of the accounts should be tied to a member id,
probably a guid. But the member will not want to remember nor lookup this id - they will want to use a username. What
you can do in EntityDb is make a lease for members where the `Scope` is `Global`, the `Label`
is `Username`, and the `Value` is whatever username the member wants to use. If an attempt to commit a source is made
that would violate the uniqueness constraint, the commit will fail. (However, this is bad UX, so the bank
should check that the username is available before attempting to commit and give immediate feedback to choose a
different username).

### State

A state is an object at a given point in time. Each state has a unique identifier, and across time each state
has a multiple versions. Together, the identifier and a single version are called a pointer, and you can refer to a
state
at a given point in time by using the associated pointer.

The default version, `Version.Zero` is given special meaning in certain scenarios. When attempting to load a state, it
means
that the state does not exist. When attempting to modify a state, it means that the previous version of the state is
irrelevant.

The balance on your bank account is a state. You can build that state by summing all of the deposits and
withdrawls on your account. If you look at the bank statements, you will most likely see the state of each bank
account for that statement, along with all of the deposits, withdrawls, and interest.

### Stream Repository

A stream repository is specialized at producing sources when the order of messages
is out of the control of the application.

A great use case for a stream is to capture events coming from 3rd party, or even from
a different domain of your own suite of applications.

Sorry - I have not yet completed my thoughts of how to apply the banking metaphor here.

### Entity Repository

An entity repository is specialized at producing sources when the order of messages
is completely in the control of the application, and consuming sources to generate
a state that determines the validity of requests.

A great use case for an entity is to capture commands.

In the banking example, there are multiple entities. You have a membership at the bank. That's an entity. You probably
have a checking account. That's an entity. And you might even have a savings account. That is also an entity!

### Projection Repository

A projection repository does not produce sources, it only consumes them.

In the banking example, one example of a projection could be your entire balance across all accounts. It can be
anything, though!
You are not constrained in what data you want to use for your projection.
