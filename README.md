# EntityDb.NET [![codecov](https://codecov.io/gh/entitydb-io/EntityDb.NET/branch/main/graph/badge.svg?token=2IK4D211SY)](https://codecov.io/gh/entitydb-io/EntityDb.NET) [![CodeFactor](https://www.codefactor.io/repository/github/entitydb-io/entitydb.net/badge)](https://www.codefactor.io/repository/github/entitydb-io/entitydb.net)

At its core, EntityDb.NET is a set of abstractions and implementations for the Event Sourcing pattern, with the added ability to enforce uniqueness constraints and the ability to tag resources.

## What is Event Sourcing?

Event Sourcing centers around the idea that your source of truth is _not_ the current state, it is all of the deltas that add up to the current state. Consider your personal bank - which of these options do you think it their source of truth for account balances?

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

The answer is pretty obvious if you go check your bank statement. They keep a set of transactions, and regurgitate that information on the statement. (If I'm wrong, you should consider getting a new bank, ASAP!)

## How does EntityDb.NET implement Event Sourcing?

There are three core sets of data:

1. Sources
2. Commands
3. Facts

### Sources

A source is the origin of a transaction. If the transaction was initiated by an HTTP API, for example, you might want record the headers and connection information of the request.

### Commands

A command represents the intent to perform some operation on the state. Going back to the bank account example, one command could be `PerformDeposit` while another could be `PerformWithdrawl`. The things that you can do are commands.

### Facts

A fact is the result of executing a command on the state. In our bank account example, the fact produced by `PerformDeposit` would be `DepositPerformed`; for `PerformWithdrawl` it would be `WithdrawlPerformed`. However, commands and facts **DO NOT** need to be one-to-one. A single command can yield as many facts as are needed. You should keep your facts _bite sized_/_small_.

### Tying it all together

The source, commands, and facts, are all tied together under one transient object - the transaction. A transaction can have exactly one source and can have many commands; each command can have many facts. When you need to commit your changes, you commit the transaction - it's all or nothing.
