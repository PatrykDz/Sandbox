import React, { useState, useEffect, useRef } from 'react';
import { motion } from 'framer-motion';
import { Play, Pause, RotateCcw, Settings, Heart, Wind, Moon, Zap } from 'lucide-react';

const BreathPacer = () => {
  const [isActive, setIsActive] = useState(false);
  const [currentPhase, setCurrentPhase] = useState('inhale'); // inhale, hold, exhale, rest
  const [cycleCount, setCycleCount] = useState(0);
  const [timeInPhase, setTimeInPhase] = useState(0);
  const [selectedPattern, setSelectedPattern] = useState('calm');
  const [customPattern, setCustomPattern] = useState({ inhale: 4, hold: 4, exhale: 4, rest: 0 });
  const [sessionStats, setSessionStats] = useState({
    totalSessions: 0,
    totalBreaths: 0,
    longestSession: 0,
    favoritePattern: 'calm'
  });
  const [sessionTime, setSessionTime] = useState(0);
  
  const timerRef = useRef(null);
  const sessionTimerRef = useRef(null);

  const breathingPatterns = {
    calm: {
      name: 'Calm & Focus',
      description: 'Balanced breathing for stress relief and mental clarity',
      icon: Heart,
      color: 'from-blue-500 to-cyan-500',
      pattern: { inhale: 4, hold: 4, exhale: 4, rest: 0 },
      benefits: ['Reduces stress', 'Improves focus', 'Balances nervous system']
    },
    energize: {
      name: 'Energizing Breath',
      description: 'Invigorating pattern to boost alertness and energy',
      icon: Zap,
      color: 'from-orange-500 to-red-500',
      pattern: { inhale: 4, hold: 7, exhale: 8, rest: 0 },
      benefits: ['Increases energy', 'Boosts alertness', 'Improves circulation']
    },
    sleep: {
      name: 'Sleep Preparation',
      description: 'Relaxing rhythm to prepare your body for rest',
      icon: Moon,
      color: 'from-indigo-500 to-purple-500',
      pattern: { inhale: 4, hold: 7, exhale: 8, rest: 2 },
      benefits: ['Promotes relaxation', 'Reduces anxiety', 'Prepares for sleep']
    },
    power: {
      name: 'Power Breathing',
      description: 'Advanced pattern for experienced practitioners',
      icon: Wind,
      color: 'from-green-500 to-teal-500',
      pattern: { inhale: 6, hold: 6, exhale: 6, rest: 2 },
      benefits: ['Builds lung capacity', 'Enhances control', 'Deepens practice']
    },
    custom: {
      name: 'Custom Pattern',
      description: 'Create your own breathing rhythm',
      icon: Settings,
      color: 'from-gray-500 to-gray-600',
      pattern: customPattern,
      benefits: ['Personalized practice', 'Flexible timing', 'Your preferences']
    }
  };

  const getCurrentPattern = () => {
    return selectedPattern === 'custom' ? customPattern : breathingPatterns[selectedPattern].pattern;
  };

  const getPhaseInstruction = () => {
    switch (currentPhase) {
      case 'inhale':
        return 'Breathe In';
      case 'hold':
        return 'Hold';
      case 'exhale':
        return 'Breathe Out';
      case 'rest':
        return 'Rest';
      default:
        return 'Breathe';
    }
  };

  const getNextPhase = (current) => {
    const pattern = getCurrentPattern();
    switch (current) {
      case 'inhale':
        return pattern.hold > 0 ? 'hold' : 'exhale';
      case 'hold':
        return 'exhale';
      case 'exhale':
        return pattern.rest > 0 ? 'rest' : 'inhale';
      case 'rest':
        return 'inhale';
      default:
        return 'inhale';
    }
  };

  const getPhaseDuration = (phase) => {
    const pattern = getCurrentPattern();
    return pattern[phase] || 0;
  };

  const startBreathing = () => {
    setIsActive(true);
    setCycleCount(0);
    setCurrentPhase('inhale');
    setTimeInPhase(0);
    setSessionTime(0);
    
    // Start phase timer
    timerRef.current = setInterval(() => {
      setTimeInPhase(prev => {
        const currentPhaseDuration = getPhaseDuration(currentPhase);
        if (prev >= currentPhaseDuration - 1) {
          const nextPhase = getNextPhase(currentPhase);
          setCurrentPhase(nextPhase);
          
          // If we just completed a full cycle (exhale -> inhale or rest -> inhale)
          if ((currentPhase === 'exhale' && nextPhase === 'inhale') || 
              (currentPhase === 'rest' && nextPhase === 'inhale')) {
            setCycleCount(prevCount => prevCount + 1);
          }
          
          return 0;
        }
        return prev + 1;
      });
    }, 1000);

    // Start session timer
    sessionTimerRef.current = setInterval(() => {
      setSessionTime(prev => prev + 1);
    }, 1000);
  };

  const pauseBreathing = () => {
    setIsActive(false);
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    if (sessionTimerRef.current) {
      clearInterval(sessionTimerRef.current);
    }
  };


  const resetBreathing = () => {
    setIsActive(false);
    setCurrentPhase('inhale');
    setTimeInPhase(0);
    
    // Update session stats
    setSessionStats(prev => ({
      totalSessions: prev.totalSessions + 1,
      totalBreaths: prev.totalBreaths + cycleCount,
      longestSession: Math.max(prev.longestSession, sessionTime),
      favoritePattern: selectedPattern
    }));
    
    setCycleCount(0);
    setSessionTime(0);
    
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    if (sessionTimerRef.current) {
      clearInterval(sessionTimerRef.current);
    }
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const getCircleScale = () => {
    const pattern = getCurrentPattern();
    const currentPhaseDuration = getPhaseDuration(currentPhase);
    const progress = timeInPhase / currentPhaseDuration;
    
    switch (currentPhase) {
      case 'inhale':
        return 0.8 + (progress * 0.4); // Scale from 0.8 to 1.2
      case 'hold':
        return 1.2; // Stay at max
      case 'exhale':
        return 1.2 - (progress * 0.4); // Scale from 1.2 to 0.8
      case 'rest':
        return 0.8; // Stay at min
      default:
        return 1;
    }
  };

  const getCircleColor = () => {
    switch (currentPhase) {
      case 'inhale':
        return 'from-blue-400 to-blue-600';
      case 'hold':
        return 'from-purple-400 to-purple-600';
      case 'exhale':
        return 'from-green-400 to-green-600';
      case 'rest':
        return 'from-gray-400 to-gray-600';
      default:
        return 'from-primary-400 to-primary-600';
    }
  };

  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
      if (sessionTimerRef.current) {
        clearInterval(sessionTimerRef.current);
      }
    };
  }, []);

  return (
    <div className="tool-container">
      <div className="tool-header">
        <h1 className="tool-title">
          <Wind className="w-12 h-12 inline mr-4 text-blue-600" />
          Breath Pacer
        </h1>
        <p className="tool-subtitle">
          Activate your parasympathetic nervous system and reduce stress through guided breathing exercises. Control your breath, control your mind.
        </p>
      </div>

      {/* Session Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{sessionStats.totalSessions}</div>
          <div className="stat-label">Sessions</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{sessionStats.totalBreaths}</div>
          <div className="stat-label">Total Breaths</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{formatTime(sessionStats.longestSession)}</div>
          <div className="stat-label">Longest Session</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{cycleCount}</div>
          <div className="stat-label">Current Breaths</div>
        </div>
      </div>

      <div className="interactive-area">
        {/* Pattern Selection */}
        {!isActive && (
          <div className="mb-8">
            <h3 className="text-xl font-bold text-center mb-4 text-gray-800">
              Choose Your Breathing Pattern
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
              {Object.entries(breathingPatterns).map(([key, pattern]) => (
                <motion.div
                  key={key}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  className={`card cursor-pointer transition-all ${
                    selectedPattern === key ? 'ring-2 ring-primary-500 bg-primary-50' : ''
                  }`}
                  onClick={() => setSelectedPattern(key)}
                >
                  <div className={`w-10 h-10 bg-gradient-to-br ${pattern.color} rounded-lg flex items-center justify-center mb-3`}>
                    <pattern.icon className="w-5 h-5 text-white" />
                  </div>
                  <h4 className="font-bold text-gray-800 mb-1">{pattern.name}</h4>
                  <p className="text-sm text-gray-600 mb-3">{pattern.description}</p>
                  <div className="text-xs text-gray-500 mb-2">
                    {pattern.pattern.inhale}:{pattern.pattern.hold}:{pattern.pattern.exhale}
                    {pattern.pattern.rest > 0 && `:${pattern.pattern.rest}`}
                  </div>
                  <div className="flex flex-wrap gap-1">
                    {pattern.benefits.slice(0, 2).map((benefit, index) => (
                      <span key={index} className="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">
                        {benefit}
                      </span>
                    ))}
                  </div>
                </motion.div>
              ))}
            </div>

            {/* Custom Pattern Settings */}
            {selectedPattern === 'custom' && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="bg-gray-50 rounded-lg p-4 mb-4"
              >
                <h4 className="font-semibold mb-3">Custom Pattern Settings</h4>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {Object.entries(customPattern).map(([phase, duration]) => (
                    <div key={phase}>
                      <label className="block text-sm font-medium text-gray-700 mb-1 capitalize">
                        {phase} (seconds)
                      </label>
                      <input
                        type="number"
                        min="0"
                        max="20"
                        value={duration}
                        onChange={(e) => setCustomPattern(prev => ({
                          ...prev,
                          [phase]: parseInt(e.target.value) || 0
                        }))}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                      />
                    </div>
                  ))}
                </div>
              </motion.div>
            )}
          </div>
        )}

        {/* Breathing Circle */}
        <div className="text-center">
          {isActive && (
            <div className="mb-4">
              <div className="text-sm text-gray-600 mb-2">Session Time: {formatTime(sessionTime)}</div>
              <div className="text-lg font-semibold text-gray-800 mb-4">
                {getPhaseInstruction()}
              </div>
            </div>
          )}

          <motion.div
            className={`w-64 h-64 bg-gradient-to-br ${getCircleColor()} rounded-full flex items-center justify-center mx-auto mb-8 shadow-lg`}
            animate={{
              scale: getCircleScale(),
            }}
            transition={{
              duration: 1,
              ease: "easeInOut"
            }}
          >
            <div className="text-white text-center">
              <div className="text-2xl font-light mb-2">
                {isActive ? getPhaseInstruction() : 'Ready'}
              </div>
              {isActive && (
                <div className="text-lg opacity-75">
                  {getPhaseDuration(currentPhase) - timeInPhase}s
                </div>
              )}
            </div>
          </motion.div>

          {/* Controls */}
          <div className="controls">
            {!isActive ? (
              <button onClick={startBreathing} className="btn btn-primary">
                <Play className="w-4 h-4 mr-2" />
                Start Breathing
              </button>
            ) : (
              <>
                <button onClick={pauseBreathing} className="btn btn-secondary">
                  <Pause className="w-4 h-4 mr-2" />
                  Pause
                </button>
                <button onClick={resetBreathing} className="btn btn-secondary">
                  <RotateCcw className="w-4 h-4 mr-2" />
                  End Session
                </button>
              </>
            )}
          </div>

          {/* Current Pattern Display */}
          {isActive && (
            <div className="mt-6 bg-white/50 rounded-lg p-4">
              <div className="text-sm text-gray-600 mb-2">Current Pattern:</div>
              <div className="font-semibold text-gray-800">
                {breathingPatterns[selectedPattern].name}
              </div>
              <div className="text-sm text-gray-600">
                Inhale {getCurrentPattern().inhale}s • 
                {getCurrentPattern().hold > 0 && ` Hold ${getCurrentPattern().hold}s • `}
                Exhale {getCurrentPattern().exhale}s
                {getCurrentPattern().rest > 0 && ` • Rest ${getCurrentPattern().rest}s`}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Benefits Section */}
      <div className="mt-8 bg-gradient-to-br from-blue-50 to-cyan-50 rounded-lg p-6">
        <h3 className="text-xl font-bold text-gray-800 mb-4">
          The Power of Controlled Breathing
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm text-gray-700">
          <div>
            <h4 className="font-semibold mb-2">Immediate Effects:</h4>
            <ul className="space-y-1">
              <li>• Activates parasympathetic nervous system</li>
              <li>• Lowers heart rate and blood pressure</li>
              <li>• Reduces cortisol (stress hormone) levels</li>
              <li>• Increases oxygen efficiency</li>
              <li>• Calms the mind and improves focus</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Long-term Benefits:</h4>
            <ul className="space-y-1">
              <li>• Improves emotional regulation</li>
              <li>• Enhances respiratory function</li>
              <li>• Builds stress resilience</li>
              <li>• Better sleep quality</li>
              <li>• Increased mindfulness and presence</li>
            </ul>
          </div>
        </div>
        <div className="mt-4 p-4 bg-blue-100 rounded-lg">
          <p className="text-sm text-blue-800 italic">
            <strong>Pro Tip:</strong> Practice breathing exercises for just 5-10 minutes daily to see significant improvements in stress levels, focus, and overall wellbeing. The 4-7-8 pattern is particularly effective for reducing anxiety and preparing for sleep.
          </p>
        </div>
      </div>
    </div>
  );
};

export default BreathPacer;