namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class FirestoreEndpoints
{
    public static void MapFirestoreEndpoints(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/firestore/ping", async (FirestoreDb db) =>
        {
            await db.ListRootCollectionsAsync()
                    .GetAsyncEnumerator()
                    .MoveNextAsync()
                    .ConfigureAwait(false);

            return Results.Ok();
        });

        apiGroup.MapPost("/firestore/users", async (CreateFirestoreUserRequest request, FirestoreDb db) =>
        {
            var docRef = db.Collection("users").Document();

            await docRef.SetAsync(new Dictionary<string, object>
            {
                ["Id"] = docRef.Id,
                ["Name"] = request.Name,
                ["CreatedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
            }).ConfigureAwait(false);

            return Results.Created($"/firestore/users/{docRef.Id}", new { docRef.Id });
        });

        apiGroup.MapGet("/firestore/users", async (FirestoreDb db) =>
        {
            var snapshot = await db.Collection("users")
                                   .GetSnapshotAsync()
                                   .ConfigureAwait(false);

            var users = snapshot.Documents
                                .Select(d => d.ToDictionary())
                                .ToList();

            return Results.Ok(users);
        });
    }
}