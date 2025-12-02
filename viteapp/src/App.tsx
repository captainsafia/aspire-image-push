import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [data, setData] = useState<any>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetch('/api/data')
      .then(res => res.json())
      .then(data => {
        setData(data)
        setLoading(false)
      })
      .catch(err => {
        console.error('Error fetching data:', err)
        setLoading(false)
      })
  }, [])

  return (
    <div className="App">
      <h1>Vite + React + Python</h1>
      <div className="card">
        {loading ? (
          <p>Loading...</p>
        ) : data ? (
          <>
            <h2>{data.message}</h2>
            <ul>
              {data.items?.map((item: string, index: number) => (
                <li key={index}>{item}</li>
              ))}
            </ul>
          </>
        ) : (
          <p>Failed to load data</p>
        )}
      </div>
      <p className="read-the-docs">
        Frontend built with Vite, served by Python FastAPI
      </p>
    </div>
  )
}

export default App
