import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [data, setData] = useState<any>(null)
  const [info, setInfo] = useState<any>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([
      fetch('/api/data').then(res => res.json()),
      fetch('/api/info').then(res => res.json())
    ])
      .then(([dataRes, infoRes]) => {
        setData(dataRes)
        setInfo(infoRes)
        setLoading(false)
      })
      .catch(err => {
        console.error('Error fetching data:', err)
        setLoading(false)
      })
  }, [])

  return (
    <div className="App">
      <h1>Vite + React (Standalone)</h1>
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
            {info && (
              <div className="info">
                <p><strong>Service:</strong> {info.service}</p>
                <p><strong>Version:</strong> {info.version}</p>
                <p><strong>Mode:</strong> {info.mode}</p>
              </div>
            )}
          </>
        ) : (
          <p>Failed to load data</p>
        )}
      </div>
      <p className="read-the-docs">
        Frontend and backend running independently
      </p>
    </div>
  )
}

export default App
