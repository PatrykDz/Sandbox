import React, { useState, useEffect, useRef } from 'react';
import { motion } from 'framer-motion';
import { Play, Pause, RotateCcw, Volume2, VolumeX, Heart, Zap, Moon } from 'lucide-react';

const MindfulnessAssistant = () => {
  const [isActive, setIsActive] = useState(false);
  const [currentSession, setCurrentSession] = useState(null);
  const [timeRemaining, setTimeRemaining] = useState(0);
  const [totalTime, setTotalTime] = useState(0);
  const [phase, setPhase] = useState('preparation'); // preparation, meditation, completion
  const [soundEnabled, setSoundEnabled] = useState(true);
  const [sessions, setSessions] = useState({
    totalSessions: 0,
    totalMinutes: 0,
    weeklyStreak: 0,
    averageRating: 0
  });
  const [currentRating, setCurrentRating] = useState(0);
  const [showRating, setShowRating] = useState(false);
  
  const timerRef = useRef(null);

  const meditationTypes = [
    {
      id: 'mindfulness',
      name: 'Mindful Breathing',
      duration: 10,
      description: 'Focus on your breath to center your mind and reduce stress',
      icon: Heart,
      color: 'from-green-500 to-teal-600',
      guidance: [
        "Find a comfortable position and close your eyes",
        "Take three deep breaths to settle in",
        "Now breathe naturally and focus on the sensation of breathing",
        "When your mind wanders, gently return to the breath",
        "Notice the air flowing in and out of your nostrils",
        "Allow thoughts to come and go without judgment"
      ]
    },
    {
      id: 'body-scan',
      name: 'Body Scan',
      duration: 15,
      description: 'Progressive relaxation technique to release physical tension',
      icon: Zap,
      color: 'from-blue-500 to-cyan-600',
      guidance: [
        "Lie down or sit comfortably with eyes closed",
        "Start by noticing your breathing",
        "Now focus on your toes, notice any sensations",
        "Slowly move your attention up through your feet",
        "Continue scanning up through your legs, torso, arms",
        "End by noticing your head and face, then your whole body"
      ]
    },
    {
      id: 'loving-kindness',
      name: 'Loving Kindness',
      duration: 12,
      description: 'Cultivate compassion and positive emotions toward yourself and others',
      icon: Heart,
      color: 'from-pink-500 to-rose-600',
      guidance: [
        "Sit comfortably and bring to mind someone you love",
        "Send them wishes: 'May you be happy, may you be peaceful'",
        "Now extend these wishes to yourself",
        "Think of a neutral person and send them the same wishes",
        "Include someone you have difficulty with",
        "Finally, extend loving kindness to all living beings"
      ]
    },
    {
      id: 'sleep',
      name: 'Sleep Meditation',
      duration: 20,
      description: 'Gentle guided meditation to prepare your mind for restful sleep',
      icon: Moon,
      color: 'from-indigo-500 to-purple-600',
      guidance: [
        "Lie down in bed and make yourself comfortable",
        "Take slow, deep breaths and let your body sink into the bed",
        "Release the day's tensions with each exhale",
        "Imagine a warm, golden light surrounding you",
        "Let go of any thoughts about tomorrow",
        "Allow yourself to drift naturally toward sleep"
      ]
    }
  ];

  const startSession = (meditationType) => {
    const session = meditationTypes.find(type => type.id === meditationType);
    setCurrentSession(session);
    setTimeRemaining(session.duration * 60); // Convert to seconds
    setTotalTime(session.duration * 60);
    setPhase('preparation');
    setIsActive(true);
    
    // Start preparation phase (30 seconds)
    setTimeout(() => {
      setPhase('meditation');
      startTimer();
    }, 3000);
  };

  const startTimer = () => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    
    timerRef.current = setInterval(() => {
      setTimeRemaining(prev => {
        if (prev <= 1) {
          endSession();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const pauseSession = () => {
    setIsActive(false);
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
  };

  const resumeSession = () => {
    setIsActive(true);
    startTimer();
  };

  const endSession = () => {
    setIsActive(false);
    setPhase('completion');
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    setShowRating(true);
    
    // Update session stats
    setSessions(prev => ({
      totalSessions: prev.totalSessions + 1,
      totalMinutes: prev.totalMinutes + currentSession.duration,
      weeklyStreak: prev.weeklyStreak + 1,
      averageRating: prev.averageRating
    }));
  };

  const resetSession = () => {
    setIsActive(false);
    setCurrentSession(null);
    setTimeRemaining(0);
    setTotalTime(0);
    setPhase('preparation');
    setShowRating(false);
    setCurrentRating(0);
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
  };

  const submitRating = (rating) => {
    setCurrentRating(rating);
    setSessions(prev => ({
      ...prev,
      averageRating: (prev.averageRating * prev.totalSessions + rating) / (prev.totalSessions + 1)
    }));
    setTimeout(() => {
      setShowRating(false);
      resetSession();
    }, 2000);
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = totalTime > 0 ? ((totalTime - timeRemaining) / totalTime) * 100 : 0;

  const getCurrentGuidance = () => {
    if (!currentSession) return '';
    const guidanceIndex = Math.floor(((totalTime - timeRemaining) / totalTime) * currentSession.guidance.length);
    return currentSession.guidance[Math.min(guidanceIndex, currentSession.guidance.length - 1)];
  };

  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
    };
  }, []);

  return (
    <div className="tool-container">
      <div className="tool-header">
        <h1 className="tool-title">
          <Heart className="w-12 h-12 inline mr-4 text-green-600" />
          Mindfulness Assistant
        </h1>
        <p className="tool-subtitle">
          Reduce stress, improve focus, and enhance emotional wellbeing through guided mindfulness meditation practices backed by neuroscience research.
        </p>
      </div>

      {/* Session Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{sessions.totalSessions}</div>
          <div className="stat-label">Sessions Completed</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{sessions.totalMinutes}</div>
          <div className="stat-label">Minutes Meditated</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{sessions.weeklyStreak}</div>
          <div className="stat-label">Current Streak</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{sessions.averageRating.toFixed(1)}</div>
          <div className="stat-label">Average Rating</div>
        </div>
      </div>

      <div className="interactive-area">
        {!currentSession && (
          <div>
            <h3 className="text-2xl font-bold text-center mb-6 text-gray-800">
              Choose Your Meditation Practice
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {meditationTypes.map((meditation) => (
                <motion.div
                  key={meditation.id}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  className="card cursor-pointer group"
                  onClick={() => startSession(meditation.id)}
                >
                  <div className={`w-12 h-12 bg-gradient-to-br ${meditation.color} rounded-lg flex items-center justify-center mb-4 group-hover:scale-110 transition-transform`}>
                    <meditation.icon className="w-6 h-6 text-white" />
                  </div>
                  <h4 className="text-lg font-bold text-gray-800 mb-2">{meditation.name}</h4>
                  <p className="text-gray-600 text-sm mb-3">{meditation.description}</p>
                  <div className="flex items-center justify-between">
                    <span className="text-primary-600 font-medium">{meditation.duration} minutes</span>
                    <div className="flex items-center text-primary-600">
                      <Play className="w-4 h-4 mr-1" />
                      Start
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        )}

        {currentSession && phase === 'preparation' && (
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="text-center"
          >
            <div className={`w-24 h-24 bg-gradient-to-br ${currentSession.color} rounded-full flex items-center justify-center mx-auto mb-6`}>
              <currentSession.icon className="w-12 h-12 text-white" />
            </div>
            <h3 className="text-2xl font-bold text-gray-800 mb-4">{currentSession.name}</h3>
            <p className="text-gray-600 mb-6">Get comfortable and prepare to begin your meditation...</p>
            <div className="text-6xl font-light text-primary-600">3</div>
          </motion.div>
        )}

        {currentSession && phase === 'meditation' && (
          <div className="text-center">
            <div className="timer-display">{formatTime(timeRemaining)}</div>
            
            <div className="progress-container mb-6">
              <div className="progress-bar">
                <div className="progress-fill" style={{ width: `${progress}%` }}></div>
              </div>
            </div>

            {/* Breathing Animation */}
            <motion.div
              className="breathing-circle mx-auto mb-6"
              animate={{
                scale: [1, 1.1, 1],
                opacity: [0.8, 1, 0.8]
              }}
              transition={{
                duration: 4,
                repeat: Infinity,
                ease: "easeInOut"
              }}
            >
              <div className="breathing-text">Breathe</div>
            </motion.div>

            {/* Current Guidance */}
            <motion.p
              key={getCurrentGuidance()}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-lg text-gray-700 mb-6 max-w-md mx-auto leading-relaxed"
            >
              {getCurrentGuidance()}
            </motion.p>

            <div className="controls">
              <button
                onClick={soundEnabled ? () => setSoundEnabled(false) : () => setSoundEnabled(true)}
                className="btn btn-secondary"
              >
                {soundEnabled ? <Volume2 className="w-4 h-4" /> : <VolumeX className="w-4 h-4" />}
              </button>
              
              {isActive ? (
                <button onClick={pauseSession} className="btn btn-secondary">
                  <Pause className="w-4 h-4 mr-2" />
                  Pause
                </button>
              ) : (
                <button onClick={resumeSession} className="btn btn-primary">
                  <Play className="w-4 h-4 mr-2" />
                  Resume
                </button>
              )}
              
              <button onClick={resetSession} className="btn btn-secondary">
                <RotateCcw className="w-4 h-4 mr-2" />
                End Session
              </button>
            </div>
          </div>
        )}

        {phase === 'completion' && !showRating && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center"
          >
            <div className="text-6xl mb-4">🙏</div>
            <h3 className="text-2xl font-bold text-gray-800 mb-4">Session Complete</h3>
            <p className="text-gray-600 mb-6">
              You've completed your {currentSession.name} meditation. Take a moment to notice how you feel.
            </p>
          </motion.div>
        )}

        {showRating && (
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="text-center"
          >
            <h3 className="text-xl font-bold text-gray-800 mb-4">How was your session?</h3>
            <div className="flex justify-center space-x-2 mb-4">
              {[1, 2, 3, 4, 5].map((rating) => (
                <button
                  key={rating}
                  onClick={() => submitRating(rating)}
                  className={`w-10 h-10 rounded-full transition-colors ${
                    rating <= currentRating ? 'bg-yellow-400' : 'bg-gray-200'
                  }`}
                >
                  ⭐
                </button>
              ))}
            </div>
            {currentRating > 0 && (
              <p className="text-green-600 font-medium">Thank you for your feedback!</p>
            )}
          </motion.div>
        )}
      </div>

      {/* Benefits Section */}
      <div className="mt-8 bg-gradient-to-br from-green-50 to-teal-50 rounded-lg p-6">
        <h3 className="text-xl font-bold text-gray-800 mb-4">
          The Science of Mindfulness
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-700">
          <div>
            <h4 className="font-semibold mb-2">Mental Health Benefits:</h4>
            <ul className="space-y-1">
              <li>• Reduces anxiety and depression</li>
              <li>• Improves emotional regulation</li>
              <li>• Enhances self-awareness</li>
              <li>• Increases stress resilience</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Cognitive Benefits:</h4>
            <ul className="space-y-1">
              <li>• Improves attention and focus</li>
              <li>• Enhances working memory</li>
              <li>• Increases cognitive flexibility</li>
              <li>• Boosts creative thinking</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Physical Benefits:</h4>
            <ul className="space-y-1">
              <li>• Lowers blood pressure</li>
              <li>• Reduces inflammation</li>
              <li>• Improves sleep quality</li>
              <li>• Boosts immune function</li>
            </ul>
          </div>
        </div>
        <p className="text-sm text-gray-600 mt-4 italic">
          Regular mindfulness practice can literally rewire your brain, increasing gray matter in areas associated with learning, memory, and emotional regulation.
        </p>
      </div>
    </div>
  );
};

export default MindfulnessAssistant;