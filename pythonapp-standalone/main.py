from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI()

# Configure CORS for standalone deployment
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify your frontend domain
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/api/health")
def health_check():
    return {"status": "healthy"}

@app.get("/api/data")
def get_data():
    return {
        "message": "Hello from standalone Python FastAPI!",
        "items": ["Item 1", "Item 2", "Item 3"]
    }

@app.get("/api/info")
def get_info():
    return {
        "service": "Python FastAPI Backend",
        "version": "1.0.0",
        "mode": "standalone"
    }
