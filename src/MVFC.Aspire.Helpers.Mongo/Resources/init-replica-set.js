sleep(2000);

try {
    var config = {
        "_id": "rs0",
        "members": [{
            "_id": 0,
            "host": "localhost:27017"
        }]
    };

    var result = rs.initiate(config);
    print("Replica set initiated: " + tojson(result));

} catch (e) {
    if (e.message.includes("already initialized")) {
        print("Replica set already initialized.");
    } else {
        print("Error initiating replica set: " + e.message);
    }
}

sleep(1000);

try {
    var result = rs.status();
    print("Replica set status: " + tojson(result.ok));
} catch (e) {
    print("Error getting replica set status: " + e.message);
}
