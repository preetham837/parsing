import pytest
import sys
from pathlib import Path
sys.path.append(str(Path(__file__).parent.parent))

from fastapi.testclient import TestClient
from main import app
from database import db


class TestAPI:
    """Integration tests for the API endpoints"""
    
    def setup_method(self):
        """Create a test client for each test"""
        self.client = TestClient(app)
    
    def test_root_redirect(self):
        """Test that root redirects to swagger"""
        response = self.client.get("/", follow_redirects=False)
        assert response.status_code == 307
        assert response.headers["location"] == "/swagger"
    
    def test_cors_headers(self):
        """Test that CORS headers are properly set"""
        # Make a request with an Origin header to trigger CORS
        response = self.client.get(
            "/",
            headers={"Origin": "http://localhost:3000"},
            follow_redirects=False
        )
        assert response.status_code == 307
        # CORS headers should be present in the response
        assert "access-control-allow-origin" in response.headers
        assert response.headers["access-control-allow-origin"] == "*"
        assert "access-control-allow-credentials" in response.headers
    
    # Add your API tests here