# EntityDb.NET [![Codacy Badge](https://app.codacy.com/project/badge/Coverage/d9c2b2e4e1ba42918ffeb2377d35bfab)](https://www.codacy.com/gh/entitydb-io/EntityDb.NET/dashboard?utm_source=github.com&utm_medium=referral&utm_content=entitydb-io/EntityDb.NET&utm_campaign=Badge_Coverage) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/d9c2b2e4e1ba42918ffeb2377d35bfab)](https://www.codacy.com/gh/entitydb-io/EntityDb.NET/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=entitydb-io/EntityDb.NET&amp;utm_campaign=Badge_Grade)

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

There are two core sets of data:

1. Sources
2. Commands

### Sources

A source is the origin of a transaction. If the transaction was initiated by an HTTP API, for example, you might want
record the headers and connection information of the request.

### Commands

A command represents the intent to perform some operation on the state. Going back to the bank account example, one
command could be `PerformDeposit` while another could be `PerformWithdrawl`. The things that you can do are commands.

### Tying it all together

The source and commands are all tied together under one transient object - the transaction. A transaction can have
exactly one source and can have many commands; each command can have many facts. When you need to commit your changes,
you commit the transaction - it's all or nothing.
