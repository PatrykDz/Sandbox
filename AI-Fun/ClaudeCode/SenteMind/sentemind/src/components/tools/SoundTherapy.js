import React, { useState, useEffect, useRef } from 'react';
import { motion } from 'framer-motion';
import { Play, Pause, Volume2, VolumeX, Headphones, Brain, Moon, Waves } from 'lucide-react';

const SoundTherapy = () => {
  const [currentTrack, setCurrentTrack] = useState(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [volume, setVolume] = useState(0.7);
  const [sessionTime, setSessionTime] = useState(0);
  const [selectedCategory, setSelectedCategory] = useState('focus');
  const [sessions, setSessions] = useState({
    totalSessions: 0,
    totalMinutes: 0,
    favoriteCategory: 'focus',
    longestSession: 0
  });

  const audioRefs = useRef({});
  const sessionTimerRef = useRef(null);

  const soundCategories = {
    focus: {
      name: 'Focus & Concentration',
      description: 'Binaural beats and ambient sounds to enhance cognitive performance',
      icon: Brain,
      color: 'from-blue-500 to-indigo-600',
      tracks: [
        {
          id: 'alpha-waves',
          name: 'Alpha Waves (10Hz)',
          description: 'Promotes relaxed focus and creativity',
          duration: '30:00',
          frequency: '10Hz',
          benefits: ['Enhanced creativity', 'Relaxed awareness', 'Improved learning']
        },
        {
          id: 'beta-waves',
          name: 'Beta Waves (20Hz)',
          description: 'Boosts alertness and concentration',
          duration: '25:00',
          frequency: '20Hz',
          benefits: ['Increased focus', 'Mental alertness', 'Problem solving']
        },
        {
          id: 'gamma-waves',
          name: 'Gamma Waves (40Hz)',
          description: 'Peak cognitive performance and memory',
          duration: '20:00',
          frequency: '40Hz',
          benefits: ['Peak performance', 'Enhanced memory', 'Cognitive binding']
        }
      ]
    },
    relaxation: {
      name: 'Relaxation & Stress Relief',
      description: 'Calming sounds to reduce stress and promote relaxation',
      icon: Waves,
      color: 'from-green-500 to-teal-600',
      tracks: [
        {
          id: 'ocean-waves',
          name: 'Ocean Waves',
          description: 'Natural ocean sounds for deep relaxation',
          duration: '60:00',
          frequency: 'Natural',
          benefits: ['Stress reduction', 'Deep relaxation', 'Natural calm']
        },
        {
          id: 'rain-forest',
          name: 'Rainforest Ambience',
          description: 'Tropical rainforest with gentle rain',
          duration: '45:00',
          frequency: 'Natural',
          benefits: ['Mental restoration', 'Anxiety relief', 'Connection to nature']
        },
        {
          id: 'theta-waves',
          name: 'Theta Waves (6Hz)',
          description: 'Deep meditation and stress relief',
          duration: '40:00',
          frequency: '6Hz',
          benefits: ['Deep meditation', 'Stress relief', 'Emotional healing']
        }
      ]
    },
    sleep: {
      name: 'Sleep & Recovery',
      description: 'Gentle frequencies to promote restful sleep',
      icon: Moon,
      color: 'from-purple-500 to-indigo-600',
      tracks: [
        {
          id: 'delta-waves',
          name: 'Delta Waves (2Hz)',
          description: 'Deep sleep and recovery frequencies',
          duration: '480:00',
          frequency: '2Hz',
          benefits: ['Deep sleep', 'Physical recovery', 'Growth hormone release']
        },
        {
          id: 'brown-noise',
          name: 'Brown Noise',
          description: 'Low-frequency noise for sleep',
          duration: '360:00',
          frequency: 'Broadband',
          benefits: ['Sleep induction', 'Noise masking', 'Relaxation']
        },
        {
          id: 'night-sounds',
          name: 'Peaceful Night',
          description: 'Gentle crickets and night ambience',
          duration: '420:00',
          frequency: 'Natural',
          benefits: ['Natural sleep', 'Circadian rhythm', 'Peaceful rest']
        }
      ]
    },
    meditation: {
      name: 'Meditation & Mindfulness',
      description: 'Sacred sounds and frequencies for deeper practice',
      icon: Headphones,
      color: 'from-orange-500 to-red-500',
      tracks: [
        {
          id: 'tibetan-bowls',
          name: 'Tibetan Singing Bowls',
          description: 'Traditional healing frequencies',
          duration: '35:00',
          frequency: '432Hz',
          benefits: ['Chakra alignment', 'Energy healing', 'Deep meditation']
        },
        {
          id: 'om-chanting',
          name: 'Om Chanting (108 times)',
          description: 'Sacred sound for spiritual practice',
          duration: '27:00',
          frequency: '136.1Hz',
          benefits: ['Spiritual connection', 'Mind stillness', 'Inner peace']
        },
        {
          id: 'nature-meditation',
          name: 'Forest Meditation',
          description: 'Birds and gentle wind for mindful presence',
          duration: '30:00',
          frequency: 'Natural',
          benefits: ['Present moment awareness', 'Nature connection', 'Mindful breathing']
        }
      ]
    }
  };

  const playTrack = (track) => {
    if (currentTrack?.id === track.id && isPlaying) {
      pauseTrack();
      return;
    }

    // Stop current track if playing
    if (currentTrack && audioRefs.current[currentTrack.id]) {
      audioRefs.current[currentTrack.id].pause();
    }

    setCurrentTrack(track);
    setIsPlaying(true);
    setSessionTime(0);

    // Start session timer
    sessionTimerRef.current = setInterval(() => {
      setSessionTime(prev => prev + 1);
    }, 1000);

    // Simulate audio playback (in real app, would use actual audio files)
    console.log(`Playing: ${track.name}`);
  };

  const pauseTrack = () => {
    setIsPlaying(false);
    if (sessionTimerRef.current) {
      clearInterval(sessionTimerRef.current);
    }

    // Update session stats
    if (sessionTime > 0) {
      setSessions(prev => ({
        totalSessions: prev.totalSessions + 1,
        totalMinutes: prev.totalMinutes + Math.floor(sessionTime / 60),
        favoriteCategory: selectedCategory,
        longestSession: Math.max(prev.longestSession, sessionTime)
      }));
    }
  };

  const stopTrack = () => {
    setIsPlaying(false);
    setCurrentTrack(null);
    setSessionTime(0);
    if (sessionTimerRef.current) {
      clearInterval(sessionTimerRef.current);
    }
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const getCurrentCategoryTracks = () => {
    return soundCategories[selectedCategory].tracks;
  };

  useEffect(() => {
    return () => {
      if (sessionTimerRef.current) {
        clearInterval(sessionTimerRef.current);
      }
    };
  }, []);

  return (
    <div className="tool-container">
      <div className="tool-header">
        <h1 className="tool-title">
          <Headphones className="w-12 h-12 inline mr-4 text-indigo-600" />
          Sound Therapy
        </h1>
        <p className="tool-subtitle">
          Harness the power of binaural beats, nature sounds, and healing frequencies to enhance focus, promote relaxation, and optimize your mental state.
        </p>
      </div>

      {/* Session Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{sessions.totalSessions}</div>
          <div className="stat-label">Sessions</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{sessions.totalMinutes}</div>
          <div className="stat-label">Minutes Listened</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{formatTime(sessions.longestSession)}</div>
          <div className="stat-label">Longest Session</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{formatTime(sessionTime)}</div>
          <div className="stat-label">Current Session</div>
        </div>
      </div>

      <div className="interactive-area">
        {/* Current Track Player */}
        {currentTrack && (
          <div className="mb-8 bg-gradient-to-r from-indigo-500 to-purple-600 rounded-lg p-6 text-white">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h3 className="text-xl font-bold">{currentTrack.name}</h3>
                <p className="text-indigo-100 text-sm">{currentTrack.description}</p>
                <div className="flex items-center space-x-4 mt-2 text-sm text-indigo-200">
                  <span>Frequency: {currentTrack.frequency}</span>
                  <span>Duration: {currentTrack.duration}</span>
                </div>
              </div>
              <div className="text-right">
                <div className="text-2xl font-light mb-2">{formatTime(sessionTime)}</div>
                <div className="text-sm text-indigo-200">Session Time</div>
              </div>
            </div>

            {/* Player Controls */}
            <div className="flex items-center justify-center space-x-4 mb-4">
              <button
                onClick={() => playTrack(currentTrack)}
                className="w-12 h-12 bg-white/20 rounded-full flex items-center justify-center hover:bg-white/30 transition-colors"
              >
                {isPlaying ? <Pause className="w-6 h-6" /> : <Play className="w-6 h-6 ml-1" />}
              </button>
              
              <button
                onClick={stopTrack}
                className="px-4 py-2 bg-white/20 rounded-lg hover:bg-white/30 transition-colors text-sm"
              >
                Stop
              </button>
            </div>

            {/* Volume Control */}
            <div className="sound-controls">
              <div className="volume-control">
                <VolumeX className="w-4 h-4 text-indigo-200" />
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={volume}
                  onChange={(e) => setVolume(parseFloat(e.target.value))}
                  className="volume-slider"
                />
                <Volume2 className="w-4 h-4 text-indigo-200" />
              </div>
            </div>

            {/* Current Track Benefits */}
            <div className="mt-4">
              <div className="text-sm text-indigo-200 mb-2">Benefits:</div>
              <div className="flex flex-wrap gap-2">
                {currentTrack.benefits.map((benefit, index) => (
                  <span
                    key={index}
                    className="px-3 py-1 bg-white/20 rounded-full text-xs"
                  >
                    {benefit}
                  </span>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* Category Selection */}
        <div className="mb-6">
          <h3 className="text-xl font-bold text-center mb-4 text-gray-800">
            Choose Your Sound Experience
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {Object.entries(soundCategories).map(([key, category]) => (
              <motion.button
                key={key}
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
                onClick={() => setSelectedCategory(key)}
                className={`p-4 rounded-lg text-center transition-all ${
                  selectedCategory === key
                    ? 'ring-2 ring-indigo-500 bg-indigo-50'
                    : 'bg-white hover:bg-gray-50'
                } shadow-sm`}
              >
                <div className={`w-12 h-12 bg-gradient-to-br ${category.color} rounded-lg flex items-center justify-center mx-auto mb-3`}>
                  <category.icon className="w-6 h-6 text-white" />
                </div>
                <h4 className="font-semibold text-gray-800 text-sm mb-1">{category.name}</h4>
                <p className="text-xs text-gray-600">{category.description}</p>
              </motion.button>
            ))}
          </div>
        </div>

        {/* Track List */}
        <div className="space-y-4">
          <h3 className="text-lg font-bold text-gray-800 flex items-center">
            {React.createElement(soundCategories[selectedCategory].icon, { className: "w-5 h-5 mr-2" })}
            {soundCategories[selectedCategory].name}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {getCurrentCategoryTracks().map((track) => (
              <motion.div
                key={track.id}
                whileHover={{ scale: 1.02 }}
                className={`card cursor-pointer ${
                  currentTrack?.id === track.id ? 'ring-2 ring-indigo-500 bg-indigo-50' : ''
                }`}
                onClick={() => playTrack(track)}
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex-1">
                    <h4 className="font-bold text-gray-800 mb-1">{track.name}</h4>
                    <p className="text-sm text-gray-600 mb-2">{track.description}</p>
                    <div className="flex items-center space-x-3 text-xs text-gray-500">
                      <span>🎵 {track.frequency}</span>
                      <span>⏱️ {track.duration}</span>
                    </div>
                  </div>
                  <button className="w-10 h-10 bg-indigo-100 rounded-full flex items-center justify-center hover:bg-indigo-200 transition-colors">
                    {currentTrack?.id === track.id && isPlaying ? (
                      <Pause className="w-4 h-4 text-indigo-600" />
                    ) : (
                      <Play className="w-4 h-4 text-indigo-600 ml-0.5" />
                    )}
                  </button>
                </div>

                <div className="border-t pt-3">
                  <div className="text-xs text-gray-500 mb-2">Benefits:</div>
                  <div className="flex flex-wrap gap-1">
                    {track.benefits.slice(0, 2).map((benefit, index) => (
                      <span
                        key={index}
                        className="px-2 py-1 bg-gray-100 text-gray-600 rounded text-xs"
                      >
                        {benefit}
                      </span>
                    ))}
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        </div>

        {/* Usage Instructions */}
        <div className="mt-8 bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <div className="flex items-start space-x-3">
            <div className="text-yellow-600 text-xl">💡</div>
            <div>
              <h4 className="font-semibold text-yellow-800 mb-2">For Best Results:</h4>
              <ul className="text-sm text-yellow-700 space-y-1">
                <li>• Use quality headphones for binaural beats (stereo separation required)</li>
                <li>• Start with lower volumes and adjust comfortably</li>
                <li>• Find a quiet, comfortable environment</li>
                <li>• Use consistently for 15-30 minutes daily</li>
                <li>• Combine with deep breathing or meditation for enhanced effects</li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      {/* Benefits Section */}
      <div className="mt-8 bg-gradient-to-br from-indigo-50 to-purple-50 rounded-lg p-6">
        <h3 className="text-xl font-bold text-gray-800 mb-4">
          The Science of Sound Therapy
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm text-gray-700">
          <div>
            <h4 className="font-semibold mb-2">Binaural Beats:</h4>
            <ul className="space-y-1">
              <li>• <strong>Delta (1-4Hz):</strong> Deep sleep, healing, regeneration</li>
              <li>• <strong>Theta (4-8Hz):</strong> Meditation, creativity, memory</li>
              <li>• <strong>Alpha (8-13Hz):</strong> Relaxed focus, learning, flow states</li>
              <li>• <strong>Beta (13-30Hz):</strong> Alert thinking, concentration</li>
              <li>• <strong>Gamma (30-100Hz):</strong> Peak awareness, binding consciousness</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Therapeutic Benefits:</h4>
            <ul className="space-y-1">
              <li>• Reduced anxiety and stress hormones</li>
              <li>• Enhanced focus and cognitive performance</li>
              <li>• Improved sleep quality and duration</li>
              <li>• Increased neuroplasticity and learning</li>
              <li>• Balanced brainwave activity</li>
            </ul>
          </div>
        </div>
        <div className="mt-4 p-4 bg-indigo-100 rounded-lg">
          <p className="text-sm text-indigo-800 italic">
            <strong>Scientific Note:</strong> Binaural beats work by playing slightly different frequencies in each ear, causing the brain to perceive a third frequency equal to the mathematical difference. This can help entrain brainwaves to desired states for optimal mental performance.
          </p>
        </div>
      </div>
    </div>
  );
};

export default SoundTherapy;