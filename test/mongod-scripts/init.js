rs.initiate(
    {
        _id: "entitydb",
        version: 1,
        members: [
            { _id: 0, host : "127.0.0.1:27017" }
        ]
    }
);