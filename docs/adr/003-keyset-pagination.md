# ADR-003: Keyset (cursor) pagination for list endpoints

- **Status:** Accepted (API-level decision, consistent with PR-05 and O1)
- **Date:** 2026-10-24

## Context
`GET /api/v1/products` must page through 10k+ items (and more later) while the data changes.

## Decision
Use keyset pagination ordered by the primary key (`_id`, an ObjectId which is roughly insertion-ordered):

`GET /products?limit=20&cursor=<opaque>` -> `{ items: [...], nextCursor: "..." | null }`

The query is `WHERE _id > :lastSeen ORDER BY _id LIMIT n+1`. The extra row tells us whether another page exists. The cursor is the base64url of the last key: opaque to clients, so the encoding can change.

## Why not offset/skip
- `skip(N)` makes the database walk and discard N entries: cost grows linearly with page depth.
- Inserts/deletes between requests shift the window, so clients see duplicates or miss items.

## Trade-offs
- No "jump to page 37" and no total count (a count is a separate, expensive query; add a cached estimate if the UI needs it).
- Sorting by anything other than the key (price, name) needs a compound cursor (value + id) and a matching compound index. Deferred until search (optional Elasticsearch service) is needed.
- Limit is clamped to 1..100 (default 20) so a client cannot request an unbounded page.
