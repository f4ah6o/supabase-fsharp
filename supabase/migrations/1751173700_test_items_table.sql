-- Create test_items table for integration property-based tests
CREATE TABLE IF NOT EXISTS test_items (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    value INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create index on name for faster lookups during tests
CREATE INDEX IF NOT EXISTS idx_test_items_name ON test_items(name);

-- Enable RLS
ALTER TABLE test_items ENABLE ROW LEVEL SECURITY;

-- Allow all operations for service role (used in tests)
CREATE POLICY "Allow all operations for service role"
    ON test_items FOR ALL
    USING (true)
    WITH CHECK (true);

-- Comment on table
COMMENT ON TABLE test_items IS 'Table used for property-based integration tests';
