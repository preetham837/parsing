import pytest
import sys
from pathlib import Path
sys.path.append(str(Path(__file__).parent.parent))

from database import InMemoryDatabase


class TestInMemoryDatabase:
    """Unit tests for the InMemoryDatabase class"""
    
    def setup_method(self):
        """Create a fresh database instance for each test"""
        self.db = InMemoryDatabase()
    
    def test_initial_state(self):
        """Test that database initializes correctly"""
        # Verify database instance exists
        assert self.db is not None
        assert hasattr(self.db, '_lock')
    
    # Add your database tests here