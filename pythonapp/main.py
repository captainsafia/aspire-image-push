from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
import os

app = FastAPI()

# API endpoints
@app.get("/api/health")
def health_check():
    return {"status": "healthy"}

@app.get("/api/data")
def get_data():
    return {
        "message": "Hello from Python FastAPI!",
        "items": ["Item 1", "Item 2", "Item 3"]
    }

# Serve static files from the Vite build
static_dir = "./static"
if os.path.exists(static_dir):
    app.mount("/assets", StaticFiles(directory=f"{static_dir}/assets"), name="assets")
    
    @app.get("/{full_path:path}")
    def serve_spa(full_path: str):
        # Serve index.html for all routes (SPA routing)
        if full_path == "" or not full_path.startswith("api"):
            index_path = os.path.join(static_dir, "index.html")
            if os.path.exists(index_path):
                return FileResponse(index_path)
        return {"error": "Not found"}
