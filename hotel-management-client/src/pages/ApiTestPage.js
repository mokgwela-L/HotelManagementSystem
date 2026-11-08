import React, { useState } from 'react';
import { roomAPI } from '../services/api';

function ApiTestPage() {
  const [result, setResult] = useState('');
  const [loading, setLoading] = useState(false);

  const testConnection = async () => {
    setLoading(true);
    setResult('Testing connection...');
    
    try {
      console.log('Testing API connection...');
      const response = await roomAPI.getAll();
      console.log('API Response:', response);
      setResult(`✅ Success! Found ${response.data.length} rooms\n\n${JSON.stringify(response.data, null, 2)}`);
    } catch (error) {
      console.error('API Error:', error);
      setResult(`❌ Error: ${error.message}\n\nCheck console for details`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '50px', maxWidth: '800px', margin: '0 auto' }}>
      <h1>API Connection Test</h1>
      <button 
        onClick={testConnection}
        disabled={loading}
        style={{
          padding: '15px 30px',
          background: '#667eea',
          color: 'white',
          border: 'none',
          borderRadius: '5px',
          fontSize: '16px',
          cursor: loading ? 'not-allowed' : 'pointer'
        }}
      >
        {loading ? 'Testing...' : 'Test API Connection'}
      </button>
      
      <pre style={{
        marginTop: '20px',
        padding: '20px',
        background: '#f5f5f5',
        borderRadius: '5px',
        whiteSpace: 'pre-wrap',
        wordWrap: 'break-word'
      }}>
        {result || 'Click button to test...'}
      </pre>
    </div>
  );
}

export default ApiTestPage;