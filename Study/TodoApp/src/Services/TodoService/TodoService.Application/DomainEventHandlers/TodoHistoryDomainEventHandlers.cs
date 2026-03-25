// History entries are now recorded directly inside the Todo aggregate root
// for each mutation and persisted atomically with the aggregate state via
// EF Core's HistoryEntries navigation property (owned collection).
//
// Domain event handlers for history are no longer required.
// This file is intentionally left empty.
