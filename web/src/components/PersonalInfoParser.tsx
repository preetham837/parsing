'use client';

import { useState, useRef } from 'react';
import { useParseMutation } from '@/store/api/parsing-api';
import { useParseIdMutation } from '@/store/api/id-parsing-api';

export function PersonalInfoParser() {
  const [inputText, setInputText] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [mode, setMode] = useState<'text' | 'image'>('text');
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const [parsePersonalInfo, { data: textData, error: textError, isLoading: textLoading, reset: resetTextData }] = useParseMutation();
  const [parseId, { data: idData, error: idError, isLoading: idLoading, reset: resetIdData }] = useParseIdMutation();

  const handleParse = async () => {
    try {
      if (mode === 'text' && inputText.trim()) {
        await parsePersonalInfo({ inputText: inputText.trim() }).unwrap();
      } else if (mode === 'image' && imageFile) {
        await parseId({ image: imageFile }).unwrap();
      }
    } catch (err) {
      console.error('Parse failed:', err);
      console.error('Error details:', JSON.stringify(err, null, 2));
      console.error('Error type:', typeof err);
      console.error('Error constructor:', err?.constructor?.name);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    setImageFile(file || null);
  };

  const handleClearFile = () => {
    setImageFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleModeChange = (newMode: 'text' | 'image') => {
    // Clear all results when switching modes
    resetTextData();
    resetIdData();
    
    // Clear input fields
    setInputText('');
    handleClearFile();
    
    // Set new mode
    setMode(newMode);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-teal-50">
      {/* Hospital Header */}
      <div className="bg-white shadow-sm border-b-2 border-teal-500">
        <div className="max-w-6xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-12 h-12 bg-teal-600 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-xl">S</span>
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-800">Sidley Hospital</h1>
                <p className="text-sm text-teal-600 font-medium">Patient Registration System</p>
              </div>
            </div>
            <div className="hidden md:flex items-center space-x-6 text-sm text-gray-600">
              <span className="flex items-center">
                <div className="w-2 h-2 bg-green-500 rounded-full mr-2"></div>
                Secure & HIPAA Compliant
              </span>
              <span className="text-gray-400">|</span>
              <span>Patient Portal</span>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto p-6 pt-8 space-y-8">
        {/* Welcome Section */}
        <div className="text-center bg-white rounded-2xl shadow-lg p-8 border border-gray-100">
          <div className="mb-6">
            <div className="w-16 h-16 bg-gradient-to-br from-teal-500 to-blue-600 rounded-2xl mx-auto mb-4 flex items-center justify-center">
              <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <h2 className="text-3xl font-bold text-gray-800 mb-3">Welcome to Patient Registration</h2>
            <p className="text-lg text-gray-600 leading-relaxed">
              Skip the paperwork! Simply provide your information using our secure digital system. 
              Upload your driver&apos;s license or enter details manually - we&apos;ll handle the rest.
            </p>
          </div>
          
          <div className="flex items-center justify-center space-x-8 text-sm text-gray-500">
            <div className="flex items-center">
              <svg className="w-5 h-5 text-teal-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
              </svg>
              HIPAA Secure
            </div>
            <div className="flex items-center">
              <svg className="w-5 h-5 text-teal-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
              Fast & Accurate
            </div>
            <div className="flex items-center">
              <svg className="w-5 h-5 text-teal-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Instant Processing
            </div>
          </div>
        </div>

        {/* Registration Method Selection */}
        <div className="bg-white rounded-2xl shadow-lg p-8 border border-gray-100">
          <div className="text-center mb-6">
            <h3 className="text-2xl font-semibold text-gray-800 mb-2">Choose Registration Method</h3>
            <p className="text-gray-600">Select how you&apos;d like to provide your information</p>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <button
              onClick={() => handleModeChange('image')}
              className={`group relative p-6 rounded-xl border-2 transition-all duration-200 ${
                mode === 'image' 
                  ? 'border-teal-500 bg-teal-50 shadow-md' 
                  : 'border-gray-200 bg-white hover:border-teal-300 hover:shadow-sm'
              }`}
            >
              <div className="text-center">
                <div className={`w-16 h-16 rounded-full mx-auto mb-4 flex items-center justify-center ${
                  mode === 'image' ? 'bg-teal-500' : 'bg-gray-100 group-hover:bg-teal-100'
                }`}>
                  <svg className={`w-8 h-8 ${mode === 'image' ? 'text-white' : 'text-gray-400 group-hover:text-teal-500'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 2v5.5a.5.5 0 0 0 .5.5h4a.5.5 0 0 0 .5-.5V2M19 4H5a1 1 0 0 0-1 1v14a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1ZM7 10h4m-4 4h6" />
                  </svg>
                </div>
                <h4 className={`text-lg font-semibold mb-2 ${mode === 'image' ? 'text-teal-700' : 'text-gray-700'}`}>
                  Upload Driver&apos;s License
                </h4>
                <p className="text-sm text-gray-600 mb-3">
                  Quick and accurate - just upload a photo of your ID
                </p>
                <div className="flex items-center justify-center space-x-4 text-xs text-gray-500">
                  <span className="flex items-center">
                    <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                    </svg>
                    Fastest
                  </span>
                  <span className="flex items-center">
                    <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                    </svg>
                    Most Accurate
                  </span>
                </div>
              </div>
              {mode === 'image' && (
                <div className="absolute top-3 right-3">
                  <div className="w-6 h-6 bg-teal-500 rounded-full flex items-center justify-center">
                    <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                  </div>
                </div>
              )}
            </button>

            <button
              onClick={() => handleModeChange('text')}
              className={`group relative p-6 rounded-xl border-2 transition-all duration-200 ${
                mode === 'text' 
                  ? 'border-blue-500 bg-blue-50 shadow-md' 
                  : 'border-gray-200 bg-white hover:border-blue-300 hover:shadow-sm'
              }`}
            >
              <div className="text-center">
                <div className={`w-16 h-16 rounded-full mx-auto mb-4 flex items-center justify-center ${
                  mode === 'text' ? 'bg-blue-500' : 'bg-gray-100 group-hover:bg-blue-100'
                }`}>
                  <svg className={`w-8 h-8 ${mode === 'text' ? 'text-white' : 'text-gray-400 group-hover:text-blue-500'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </div>
                <h4 className={`text-lg font-semibold mb-2 ${mode === 'text' ? 'text-blue-700' : 'text-gray-700'}`}>
                  Enter Information Manually
                </h4>
                <p className="text-sm text-gray-600 mb-3">
                  Type your personal details if you prefer manual entry
                </p>
                <div className="flex items-center justify-center space-x-4 text-xs text-gray-500">
                  <span className="flex items-center">
                    <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                    </svg>
                    Flexible
                  </span>
                  <span className="flex items-center">
                    <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                    </svg>
                    No ID Required
                  </span>
                </div>
              </div>
              {mode === 'text' && (
                <div className="absolute top-3 right-3">
                  <div className="w-6 h-6 bg-blue-500 rounded-full flex items-center justify-center">
                    <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                  </div>
                </div>
              )}
            </button>
          </div>
        </div>

        {/* Input Section */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
          {mode === 'text' && (
            <div className="p-8">
              <div className="flex items-center mb-6">
                <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center mr-4">
                  <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </div>
                <div>
                  <h4 className="text-xl font-semibold text-gray-800">Manual Information Entry</h4>
                  <p className="text-sm text-gray-600">Please enter your personal information as free text</p>
                </div>
              </div>
              
              <div className="space-y-4">
                <label htmlFor="inputText" className="block text-sm font-semibold text-gray-700 mb-2">
                  Personal Information
                </label>
                <textarea
                  id="inputText"
                  value={inputText}
                  onChange={(e) => setInputText(e.target.value)}
                  className="w-full h-32 p-4 border-2 border-gray-200 rounded-lg focus:ring-3 focus:ring-blue-100 focus:border-blue-500 transition-all duration-200 resize-none"
                  placeholder="Example: My name is John Smith, I live at 123 Main Street, Boston, MA 02101. My phone number is (555) 123-4567."
                />
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <div className="flex items-start">
                    <svg className="w-5 h-5 text-blue-500 mr-2 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                    <div className="text-sm text-blue-700">
                      <p className="font-medium mb-1">Please include:</p>
                      <ul className="space-y-1 text-blue-600">
                        <li>• Full name</li>
                        <li>• Complete address (street, city, state, ZIP)</li>
                        <li>• Phone number</li>
                        <li>• Any other relevant contact information</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {mode === 'image' && (
            <div className="p-8">
              <div className="flex items-center mb-6">
                <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center mr-4">
                  <svg className="w-6 h-6 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <div>
                  <h4 className="text-xl font-semibold text-gray-800">Driver&apos;s License Upload</h4>
                  <p className="text-sm text-gray-600">Upload a clear photo of your driver&apos;s license for instant processing</p>
                </div>
              </div>

              <div className="space-y-4">
                <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center bg-gray-50 hover:bg-gray-100 transition-colors duration-200">
                  <input
                    ref={fileInputRef}
                    id="imageFile"
                    type="file"
                    accept="image/*,.jpg,.jpeg,.png,.webp"
                    onChange={handleFileChange}
                    className="hidden"
                  />
                  
                  {!imageFile ? (
                    <div onClick={() => fileInputRef.current?.click()} className="cursor-pointer">
                      <svg className="w-12 h-12 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                      </svg>
                      <p className="text-lg font-medium text-gray-700 mb-2">Click to upload your driver&apos;s license</p>
                      <p className="text-sm text-gray-500">JPG, PNG, or WebP • Max 10MB</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      <div className="w-16 h-16 bg-teal-500 rounded-full mx-auto flex items-center justify-center">
                        <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                      </div>
                      <div>
                        <p className="text-lg font-semibold text-teal-700">{imageFile.name}</p>
                        <p className="text-sm text-gray-600">{Math.round(imageFile.size / 1024)} KB • Ready to process</p>
                      </div>
                      <button
                        onClick={handleClearFile}
                        className="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors duration-200"
                      >
                        <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                        Choose Different File
                      </button>
                    </div>
                  )}
                </div>

                <div className="bg-teal-50 border border-teal-200 rounded-lg p-4">
                  <div className="flex items-start">
                    <svg className="w-5 h-5 text-teal-500 mr-2 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                    <div className="text-sm text-teal-700">
                      <p className="font-medium mb-1">For best results:</p>
                      <ul className="space-y-1 text-teal-600">
                        <li>• Ensure good lighting and clear image quality</li>
                        <li>• Keep the license flat and fully visible</li>
                        <li>• Avoid shadows and glare</li>
                        <li>• All text should be clearly readable</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          <div className="bg-gray-50 px-8 py-6 border-t border-gray-100">
            <button
              onClick={handleParse}
              disabled={
                textLoading || idLoading || 
                (mode === 'text' ? !inputText.trim() : !imageFile)
              }
              className={`w-full py-4 px-6 rounded-lg font-semibold text-white transition-all duration-200 ${
                textLoading || idLoading || (mode === 'text' ? !inputText.trim() : !imageFile)
                  ? 'bg-gray-400 cursor-not-allowed'
                  : mode === 'image'
                    ? 'bg-teal-600 hover:bg-teal-700 active:bg-teal-800 shadow-md hover:shadow-lg'
                    : 'bg-blue-600 hover:bg-blue-700 active:bg-blue-800 shadow-md hover:shadow-lg'
              }`}
            >
              {(textLoading || idLoading) ? (
                <div className="flex items-center justify-center">
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Processing Your Information...
                </div>
              ) : (
                <div className="flex items-center justify-center">
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                  </svg>
                  {mode === 'text' ? 'Process Information' : 'Process Driver\'s License'}
                </div>
              )}
            </button>
          </div>
        </div>

      {/* Hospital-Styled Error Section */}
      {(textError || idError) && (
        <div className="bg-white rounded-2xl shadow-lg border border-red-200 p-8">
          <div className="flex items-center mb-4">
            <div className="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center mr-4">
              <svg className="w-7 h-7 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
            </div>
            <div>
              <h3 className="text-2xl font-semibold text-red-800">Processing Error</h3>
              <p className="text-red-600">There was an issue processing your information</p>
            </div>
          </div>
          
          <div className="bg-red-50 rounded-lg p-4 border border-red-200">
            <p className="text-red-700 text-sm">
              {(textError && 'data' in textError && textError.data) ? 
                JSON.stringify(textError.data) : 
                (idError && 'data' in idError && idError.data) ?
                  JSON.stringify(idError.data) :
                  'An error occurred while processing your request. Please try again or contact support if the issue persists.'
              }
            </p>
          </div>
          
          <div className="mt-4 flex items-center text-sm text-red-600">
            <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
            </svg>
            Need assistance? Contact Sidley Hospital support at (555) 123-4567
          </div>
        </div>
      )}

      {/* Hospital-Styled Text Parsing Results */}
      {textData && (
        <div className="bg-white rounded-2xl shadow-lg border border-green-200 p-8">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mr-4">
                <svg className="w-7 h-7 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div>
                <h3 className="text-2xl font-semibold text-green-800">Registration Successful</h3>
                <p className="text-green-600">Patient information processed successfully</p>
              </div>
            </div>
            <span className="text-sm px-3 py-1 bg-green-100 text-green-700 rounded-full font-medium">
              Manual Entry
            </span>
          </div>
          
          <div className="bg-green-50 rounded-xl p-6 mb-6 border border-green-200">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold text-green-800 mb-1">Patient Name</label>
                  <p className="text-gray-900 font-medium text-lg">{textData.data.name || 'Not provided'}</p>
                </div>
                <div>
                  <label className="block text-sm font-semibold text-green-800 mb-1">Street Address</label>
                  <p className="text-gray-900">{textData.data.street || 'Not provided'}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-green-800 mb-1">City</label>
                    <p className="text-gray-900">{textData.data.city || 'Not provided'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-green-800 mb-1">State</label>
                    <p className="text-gray-900">{textData.data.state || 'Not provided'}</p>
                  </div>
                </div>
              </div>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold text-green-800 mb-1">Phone Number</label>
                  <p className="text-gray-900 font-mono">{textData.data.phoneNumber || 'Not provided'}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-green-800 mb-1">ZIP Code</label>
                    <p className="text-gray-900">{textData.data.zipCode || 'Not provided'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-green-800 mb-1">Country</label>
                    <p className="text-gray-900">{textData.data.country || 'Not provided'}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="flex items-center justify-center text-sm text-green-600 bg-green-100 rounded-lg p-4">
            <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
            </svg>
            <span className="font-medium">Patient information has been securely processed and is ready for review by medical staff</span>
          </div>
        </div>
      )}

      {/* Hospital-Styled ID/License Results */}
      {idData && (
        <div className="bg-white rounded-2xl shadow-lg border border-teal-200 p-8">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-teal-100 rounded-lg flex items-center justify-center mr-4">
                <svg className="w-7 h-7 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 2v5.5a.5.5 0 0 0 .5.5h4a.5.5 0 0 0 .5-.5V2M19 4H5a1 1 0 0 0-1 1v14a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1ZM7 10h4m-4 4h6" />
                </svg>
              </div>
              <div>
                <h3 className="text-2xl font-semibold text-teal-800">Patient Registration Complete</h3>
                <p className="text-teal-600">Driver&apos;s license processed and verified</p>
              </div>
            </div>
            <span className="text-sm px-3 py-1 bg-teal-100 text-teal-700 rounded-full font-medium">
              License Upload
            </span>
          </div>

          <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
            {/* Patient Information */}
            <div className="bg-teal-50 rounded-xl p-6 border border-teal-200">
              <div className="flex items-center mb-4">
                <svg className="w-6 h-6 text-teal-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <h4 className="font-bold text-teal-900 text-lg">Patient Details</h4>
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold text-teal-800 mb-1">Full Name</label>
                  <p className="text-gray-900 font-medium text-lg">{idData.data.fullName || 'Not detected'}</p>
                </div>
                <div>
                  <label className="block text-sm font-semibold text-teal-800 mb-1">Date of Birth</label>
                  <p className="text-gray-900 font-mono">{idData.data.dateOfBirth || 'Not detected'}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-teal-800 mb-1">Gender</label>
                    <p className="text-gray-900">{idData.data.sex || 'Not detected'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-teal-800 mb-1">Height</label>
                    <p className="text-gray-900">{idData.data.height || 'Not detected'}</p>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-semibold text-teal-800 mb-1">Eye Color</label>
                  <p className="text-gray-900">{idData.data.eyeColor || 'Not detected'}</p>
                </div>
              </div>
            </div>

            {/* Address Information */}
            <div className="bg-blue-50 rounded-xl p-6 border border-blue-200">
              <div className="flex items-center mb-4">
                <svg className="w-6 h-6 text-blue-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <h4 className="font-bold text-blue-900 text-lg">Address Information</h4>
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold text-blue-800 mb-1">Street Address</label>
                  <p className="text-gray-900">{idData.data.address?.street || 'Not detected'}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-blue-800 mb-1">City</label>
                    <p className="text-gray-900">{idData.data.address?.city || 'Not detected'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-blue-800 mb-1">State</label>
                    <p className="text-gray-900">{idData.data.address?.state || 'Not detected'}</p>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-blue-800 mb-1">ZIP Code</label>
                    <p className="text-gray-900 font-mono">{idData.data.address?.zipCode || 'Not detected'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-blue-800 mb-1">Country</label>
                    <p className="text-gray-900">{idData.data.address?.country || 'Not detected'}</p>
                  </div>
                </div>
              </div>
            </div>

            {/* License Information */}
            <div className="bg-purple-50 rounded-xl p-6 border border-purple-200">
              <div className="flex items-center mb-4">
                <svg className="w-6 h-6 text-purple-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />
                </svg>
                <h4 className="font-bold text-purple-900 text-lg">License Details</h4>
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold text-purple-800 mb-1">Document Number</label>
                  <p className="text-gray-900 font-mono text-sm bg-gray-100 px-2 py-1 rounded">{idData.data.documentNumber || 'Not detected'}</p>
                </div>
                <div>
                  <label className="block text-sm font-semibold text-purple-800 mb-1">License Class</label>
                  <p className="text-gray-900">{idData.data.licenseClass || 'Not detected'}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-purple-800 mb-1">Issue Date</label>
                    <p className="text-gray-900 text-sm">{idData.data.issueDate || 'Not detected'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-purple-800 mb-1">Expiration</label>
                    <p className="text-gray-900 text-sm">{idData.data.expirationDate || 'Not detected'}</p>
                  </div>
                </div>
                <div className="space-y-2">
                  <div>
                    <label className="block text-sm font-semibold text-purple-800 mb-1">Endorsements</label>
                    <p className="text-gray-900 text-sm">{idData.data.endorsements || 'None'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-purple-800 mb-1">Restrictions</label>
                    <p className="text-gray-900 text-sm">{idData.data.restrictions || 'None'}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Verification Status and Warnings */}
          <div className="mt-8 pt-6 border-t border-gray-200">
            <div className="flex justify-center">
              {/* Verification Status */}
              <div className="bg-gray-50 rounded-lg p-4 max-w-md w-full">
                <h5 className="font-semibold text-gray-800 mb-3 flex items-center justify-center">
                  <svg className="w-5 h-5 text-gray-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  Document Verification
                </h5>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span>Barcode Present:</span>
                    <span className={idData.data.barcodePresent ? 'text-green-600 font-medium' : 'text-red-600 font-medium'}>
                      {idData.data.barcodePresent ? '✅ Verified' : '❌ Not Found'}
                    </span>
                  </div>
                  {idData.data.detectedState && (
                    <div className="flex justify-between">
                      <span>Issuing State:</span>
                      <span className="font-medium text-teal-600">{idData.data.detectedState}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Success Footer */}
          <div className="mt-8 flex items-center justify-center text-sm text-teal-600 bg-teal-100 rounded-lg p-4">
            <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
            </svg>
            <span className="font-medium">Patient registration completed successfully. All information has been securely processed for Sidley Hospital records.</span>
          </div>
        </div>
      )}

        {/* Hospital-Styled Examples Section */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8">
          <div className="flex items-center mb-6">
            <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center mr-4">
              <svg className="w-6 h-6 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
              </svg>
            </div>
            <div>
              <h3 className="text-2xl font-semibold text-gray-800">How It Works</h3>
              <p className="text-gray-600">Quick guide to registering with Sidley Hospital</p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="bg-blue-50 rounded-xl p-6 border border-blue-100">
              <div className="flex items-start mb-4">
                <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center mr-3 flex-shrink-0">
                  <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </div>
                <div>
                  <h4 className="text-lg font-semibold text-blue-800 mb-2">Manual Entry Method</h4>
                  <p className="text-blue-700 text-sm mb-3">
                    Perfect when you prefer to type your information directly
                  </p>
                </div>
              </div>
              <div className="bg-white rounded-lg p-4 border border-blue-200">
                <p className="text-gray-700 italic text-sm mb-2">
                  &ldquo;My name is Sarah Johnson, I live at 456 Oak Avenue, Springfield, IL 62701. My phone number is (217) 555-0123.&rdquo;
                </p>
                <div className="text-xs text-blue-600 bg-blue-100 rounded px-2 py-1 inline-block">
                  ✓ Extracts: Name, Address, Phone Number
                </div>
              </div>
            </div>
            
            <div className="bg-teal-50 rounded-xl p-6 border border-teal-100">
              <div className="flex items-start mb-4">
                <div className="w-8 h-8 bg-teal-500 rounded-lg flex items-center justify-center mr-3 flex-shrink-0">
                  <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <div>
                  <h4 className="text-lg font-semibold text-teal-800 mb-2">Driver&apos;s License Upload</h4>
                  <p className="text-teal-700 text-sm mb-3">
                    Most accurate and fastest way to register
                  </p>
                </div>
              </div>
              <div className="bg-white rounded-lg p-4 border border-teal-200">
                <p className="text-gray-700 text-sm mb-2">
                  Simply upload a clear photo of your driver&apos;s license. Our advanced AI will instantly extract all necessary information including:
                </p>
                <div className="text-xs text-teal-600 space-y-1">
                  <div className="bg-teal-100 rounded px-2 py-1 inline-block mr-1 mb-1">✓ Personal Details</div>
                  <div className="bg-teal-100 rounded px-2 py-1 inline-block mr-1 mb-1">✓ Address</div>
                  <div className="bg-teal-100 rounded px-2 py-1 inline-block mr-1 mb-1">✓ ID Verification</div>
                </div>
              </div>
            </div>
          </div>
          
          <div className="mt-8 pt-6 border-t border-gray-200">
            <div className="bg-gray-50 rounded-xl p-6">
              <div className="flex items-center justify-center space-x-8 text-sm">
                <div className="flex items-center text-gray-600">
                  <svg className="w-5 h-5 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                  </svg>
                  <span><strong>HIPAA Compliant:</strong> Your data is encrypted and secure</span>
                </div>
                <div className="hidden md:block w-px h-6 bg-gray-300"></div>
                <div className="flex items-center text-gray-600">
                  <svg className="w-5 h-5 text-teal-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span><strong>Powered by:</strong> GROQ AI Advanced Processing</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}