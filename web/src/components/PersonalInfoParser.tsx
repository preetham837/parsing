'use client';

import { useState } from 'react';
import { useParseMutation } from '@/store/api/parsing-api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

export function PersonalInfoParser() {
  const [inputText, setInputText] = useState('');
  const [lookupId, setLookupId] = useState('');
  const [mode, setMode] = useState<'text' | 'lookup'>('text');
  
  const [parsePersonalInfo, { data, error, isLoading }] = useParseMutation();

  const handleParse = async () => {
    try {
      if (mode === 'text' && inputText.trim()) {
        await parsePersonalInfo({ inputText: inputText.trim() }).unwrap();
      } else if (mode === 'lookup' && lookupId.trim()) {
        await parsePersonalInfo({ id: lookupId.trim() }).unwrap();
      }
    } catch (err) {
      console.error('Parse failed:', err);
    }
  };

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-6">
      <div className="text-center">
        <h1 className="text-3xl font-bold mb-2">Personal Information Parser</h1>
        <p className="text-gray-600">
          Extract personal information from text or lookup stored user data
        </p>
      </div>

      {/* Mode Selection */}
      <div className="flex justify-center space-x-4">
        <Button 
          variant={mode === 'text' ? 'default' : 'outline'}
          onClick={() => setMode('text')}
        >
          Parse Text
        </Button>
        <Button 
          variant={mode === 'lookup' ? 'default' : 'outline'}
          onClick={() => setMode('lookup')}
        >
          ID Lookup
        </Button>
      </div>

      {/* Input Section */}
      <div className="bg-white rounded-lg border p-6 space-y-4">
        {mode === 'text' ? (
          <div className="space-y-2">
            <label htmlFor="inputText" className="block text-sm font-medium">
              Enter text containing personal information:
            </label>
            <textarea
              id="inputText"
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
              className="w-full min-h-24 p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Example: My name is Jim Croce, I live in 2944 Monaco dr, Manchester, Colorado, USA, 92223. My phone number is 893-366-8888."
            />
          </div>
        ) : (
          <div className="space-y-2">
            <label htmlFor="lookupId" className="block text-sm font-medium">
              Enter user ID:
            </label>
            <Input
              id="lookupId"
              type="text"
              value={lookupId}
              onChange={(e) => setLookupId(e.target.value)}
              placeholder="jim-croce, person-1, person-2, person-3"
            />
            <p className="text-sm text-gray-500">
              Available IDs: jim-croce, person-1, person-2, person-3
            </p>
          </div>
        )}

        <Button 
          onClick={handleParse} 
          disabled={isLoading || (mode === 'text' ? !inputText.trim() : !lookupId.trim())}
          className="w-full"
        >
          {isLoading ? 'Processing...' : mode === 'text' ? 'Parse Text' : 'Lookup User'}
        </Button>
      </div>

      {/* Results Section */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h3 className="text-red-800 font-semibold mb-2">Error</h3>
          <p className="text-red-700">
            {('data' in error && error.data) 
              ? JSON.stringify(error.data) 
              : 'An error occurred while processing your request'}
          </p>
        </div>
      )}

      {data && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-6">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-green-800 font-semibold text-lg">Parsed Information</h3>
            <span className="text-sm px-2 py-1 bg-green-100 text-green-700 rounded-md">
              Source: {data.source}
            </span>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700">Name</label>
                <p className="text-gray-900 font-medium">{data.data.name || 'N/A'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Street</label>
                <p className="text-gray-900">{data.data.street || 'N/A'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">City</label>
                <p className="text-gray-900">{data.data.city || 'N/A'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">State</label>
                <p className="text-gray-900">{data.data.state || 'N/A'}</p>
              </div>
            </div>
            
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700">Country</label>
                <p className="text-gray-900">{data.data.country || 'N/A'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">ZIP Code</label>
                <p className="text-gray-900">{data.data.zipCode || 'N/A'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Phone Number</label>
                <p className="text-gray-900">{data.data.phoneNumber || 'N/A'}</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Examples Section */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
        <h3 className="text-blue-800 font-semibold mb-3">Examples</h3>
        <div className="space-y-2 text-sm">
          <div>
            <strong>Text Parsing:</strong>
            <p className="text-blue-700 italic">
              "My name is John Doe, I live at 123 Main St, Boston, MA 02101. Phone: 555-123-4567"
            </p>
          </div>
          <div>
            <strong>ID Lookup:</strong>
            <p className="text-blue-700">
              Use "jim-croce" to see Jim Croce's information from Manchester, Colorado
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}