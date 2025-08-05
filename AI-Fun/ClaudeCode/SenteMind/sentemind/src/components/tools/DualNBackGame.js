import React, { useState, useEffect, useCallback } from 'react';
import { motion } from 'framer-motion';
import { Play, Pause, RotateCcw, TrendingUp, Brain, Award } from 'lucide-react';

const DualNBackGame = () => {
  const [gameState, setGameState] = useState('ready'); // ready, playing, paused, finished
  const [level, setLevel] = useState(2);
  const [currentRound, setCurrentRound] = useState(0);
  const [totalRounds] = useState(20);
  const [sequence, setSequence] = useState([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [score, setScore] = useState({ correct: 0, total: 0 });
  const [positionMatches, setPositionMatches] = useState([]);
  const [audioMatches, setAudioMatches] = useState([]);
  const [showFeedback, setShowFeedback] = useState(null);
  const [gameStats, setGameStats] = useState({
    sessionsCompleted: 0,
    averageAccuracy: 0,
    bestLevel: 2,
    totalPlayTime: 0
  });

  const positions = [
    { id: 0, x: 1, y: 1 }, { id: 1, x: 2, y: 1 }, { id: 2, x: 3, y: 1 },
    { id: 3, x: 1, y: 2 }, { id: 4, x: 2, y: 2 }, { id: 5, x: 3, y: 2 },
    { id: 6, x: 1, y: 3 }, { id: 7, x: 2, y: 3 }, { id: 8, x: 3, y: 3 }
  ];

  const audioSounds = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];

  const generateSequence = useCallback(() => {
    const newSequence = [];
    for (let i = 0; i < totalRounds; i++) {
      newSequence.push({
        position: Math.floor(Math.random() * 9),
        audio: audioSounds[Math.floor(Math.random() * audioSounds.length)]
      });
    }
    setSequence(newSequence);
  }, [totalRounds, audioSounds]);

  const startGame = () => {
    generateSequence();
    setGameState('playing');
    setCurrentIndex(0);
    setCurrentRound(1);
    setScore({ correct: 0, total: 0 });
    setPositionMatches([]);
    setAudioMatches([]);
    setShowFeedback(null);
  };

  const pauseGame = () => {
    setGameState('paused');
  };

  const resumeGame = () => {
    setGameState('playing');
  };

  const resetGame = () => {
    setGameState('ready');
    setCurrentIndex(0);
    setCurrentRound(0);
    setSequence([]);
    setScore({ correct: 0, total: 0 });
    setPositionMatches([]);
    setAudioMatches([]);
    setShowFeedback(null);
  };

  const checkMatch = (type, isMatch) => {
    if (currentIndex < level) return; // Can't match until we have enough items
    
    const currentItem = sequence[currentIndex];
    const nBackItem = sequence[currentIndex - level];
    
    let actualMatch = false;
    if (type === 'position') {
      actualMatch = currentItem.position === nBackItem.position;
    } else if (type === 'audio') {
      actualMatch = currentItem.audio === nBackItem.audio;
    }
    
    const correct = isMatch === actualMatch;
    
    setScore(prev => ({
      correct: prev.correct + (correct ? 1 : 0),
      total: prev.total + 1
    }));
    
    // Show feedback
    setShowFeedback({ type, correct, actualMatch });
    setTimeout(() => setShowFeedback(null), 1000);
    
    if (type === 'position') {
      setPositionMatches(prev => [...prev, { round: currentIndex + 1, correct, actualMatch }]);
    } else {
      setAudioMatches(prev => [...prev, { round: currentIndex + 1, correct, actualMatch }]);
    }
  };

  const nextRound = () => {
    if (currentIndex < sequence.length - 1) {
      setCurrentIndex(prev => prev + 1);
      setCurrentRound(prev => prev + 1);
    } else {
      // Game finished
      setGameState('finished');
      const finalAccuracy = (score.correct + 1) / (score.total + 1) * 100;
      
      setGameStats(prev => ({
        sessionsCompleted: prev.sessionsCompleted + 1,
        averageAccuracy: (prev.averageAccuracy * prev.sessionsCompleted + finalAccuracy) / (prev.sessionsCompleted + 1),
        bestLevel: Math.max(prev.bestLevel, level),
        totalPlayTime: prev.totalPlayTime + 1
      }));
    }
  };

  // Auto-advance game
  useEffect(() => {
    if (gameState === 'playing' && sequence.length > 0) {
      const timer = setTimeout(() => {
        nextRound();
      }, 3000); // 3 seconds per round
      
      return () => clearTimeout(timer);
    }
  }, [gameState, currentIndex, sequence.length, nextRound]);

  const getCurrentItem = () => {
    return sequence[currentIndex] || null;
  };

  const accuracy = score.total > 0 ? (score.correct / score.total * 100).toFixed(1) : 0;

  return (
    <div className="tool-container">
      <div className="tool-header">
        <h1 className="tool-title">
          <Brain className="w-12 h-12 inline mr-4 text-primary-600" />
          Dual N-Back Training
        </h1>
        <p className="tool-subtitle">
          Boost your working memory and fluid intelligence with this scientifically proven cognitive training exercise. The dual n-back task challenges both visual and auditory working memory simultaneously.
        </p>
      </div>

      {/* Game Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{gameStats.sessionsCompleted}</div>
          <div className="stat-label">Sessions Completed</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{gameStats.averageAccuracy.toFixed(1)}%</div>
          <div className="stat-label">Average Accuracy</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{gameStats.bestLevel}</div>
          <div className="stat-label">Best Level</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{gameStats.totalPlayTime}</div>
          <div className="stat-label">Hours Trained</div>
        </div>
      </div>

      <div className="interactive-area">
        {/* Game Controls */}
        <div className="controls">
          <div className="control-group">
            <label className="control-label">N-Back Level</label>
            <select 
              value={level} 
              onChange={(e) => setLevel(parseInt(e.target.value))}
              disabled={gameState === 'playing'}
              className="px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              {[1, 2, 3, 4, 5, 6].map(n => (
                <option key={n} value={n}>{n}-Back</option>
              ))}
            </select>
          </div>

          <div className="flex gap-2">
            {gameState === 'ready' && (
              <button onClick={startGame} className="btn btn-primary">
                <Play className="w-4 h-4 mr-2" />
                Start Game
              </button>
            )}
            
            {gameState === 'playing' && (
              <button onClick={pauseGame} className="btn btn-secondary">
                <Pause className="w-4 h-4 mr-2" />
                Pause
              </button>
            )}
            
            {gameState === 'paused' && (
              <>
                <button onClick={resumeGame} className="btn btn-primary">
                  <Play className="w-4 h-4 mr-2" />
                  Resume
                </button>
                <button onClick={resetGame} className="btn btn-secondary">
                  <RotateCcw className="w-4 h-4 mr-2" />
                  Reset
                </button>
              </>
            )}
            
            {gameState === 'finished' && (
              <button onClick={resetGame} className="btn btn-primary">
                <RotateCcw className="w-4 h-4 mr-2" />
                Play Again
              </button>
            )}
          </div>
        </div>

        {/* Game Progress */}
        {(gameState === 'playing' || gameState === 'paused' || gameState === 'finished') && (
          <div className="progress-container">
            <div className="flex justify-between items-center mb-2">
              <span className="text-sm font-semibold text-gray-700">
                Round {currentRound} of {totalRounds}
              </span>
              <span className="text-sm font-semibold text-gray-700">
                Accuracy: {accuracy}%
              </span>
            </div>
            <div className="progress-bar">
              <div 
                className="progress-fill" 
                style={{ width: `${(currentRound / totalRounds) * 100}%` }}
              ></div>
            </div>
          </div>
        )}

        {/* Game Grid */}
        <div className="game-grid">
          {positions.map(pos => (
            <motion.div
              key={pos.id}
              className={`game-cell ${
                gameState === 'playing' && getCurrentItem()?.position === pos.id ? 'active' : ''
              }`}
              animate={{
                scale: gameState === 'playing' && getCurrentItem()?.position === pos.id ? 1.1 : 1,
                backgroundColor: gameState === 'playing' && getCurrentItem()?.position === pos.id ? '#6366f1' : '#f3f4f6'
              }}
              transition={{ duration: 0.2 }}
            />
          ))}
        </div>

        {/* Audio Display */}
        {gameState === 'playing' && getCurrentItem() && (
          <motion.div
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="text-center"
          >
            <div className="text-4xl font-bold text-primary-600 bg-primary-50 rounded-lg p-6 inline-block">
              {getCurrentItem().audio}
            </div>
          </motion.div>
        )}

        {/* Match Buttons */}
        {gameState === 'playing' && currentIndex >= level && (
          <div className="controls">
            <button
              onClick={() => checkMatch('position', true)}
              className="btn btn-secondary"
            >
              Position Match
            </button>
            <button
              onClick={() => checkMatch('audio', true)}
              className="btn btn-secondary"
            >
              Audio Match
            </button>
          </div>
        )}

        {/* Feedback */}
        {showFeedback && (
          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.8 }}
            className={`text-center p-4 rounded-lg ${
              showFeedback.correct ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
            }`}
          >
            <div className="font-semibold">
              {showFeedback.correct ? '✓ Correct!' : '✗ Incorrect'}
            </div>
            <div className="text-sm">
              {showFeedback.type === 'position' ? 'Position' : 'Audio'} {showFeedback.actualMatch ? 'was' : 'was not'} a match
            </div>
          </motion.div>
        )}

        {/* Game Finished */}
        {gameState === 'finished' && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center bg-primary-50 p-6 rounded-lg"
          >
            <Award className="w-16 h-16 text-primary-600 mx-auto mb-4" />
            <h3 className="text-2xl font-bold text-primary-800 mb-2">Game Complete!</h3>
            <p className="text-primary-700 mb-4">
              Final Accuracy: <span className="font-bold">{accuracy}%</span>
            </p>
            <p className="text-sm text-primary-600">
              {accuracy >= 80 ? "Excellent work! Consider increasing the N-Back level." :
               accuracy >= 60 ? "Good job! Keep practicing to improve." :
               "Keep practicing! Consistency is key to improvement."}
            </p>
          </motion.div>
        )}
      </div>

      {/* Benefits Section */}
      <div className="mt-8 bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-6">
        <h3 className="text-xl font-bold text-gray-800 mb-4 flex items-center">
          <TrendingUp className="w-6 h-6 mr-2 text-blue-600" />
          Why Dual N-Back Training Works
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-700">
          <div>
            <h4 className="font-semibold mb-2">Cognitive Benefits:</h4>
            <ul className="space-y-1">
              <li>• Improves working memory capacity</li>
              <li>• Enhances fluid intelligence</li>
              <li>• Increases attention control</li>
              <li>• Boosts problem-solving abilities</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Real-World Impact:</h4>
            <ul className="space-y-1">
              <li>• Better academic performance</li>
              <li>• Enhanced professional productivity</li>
              <li>• Improved multitasking ability</li>
              <li>• Reduced cognitive decline with age</li>
            </ul>
          </div>
        </div>
        <p className="text-sm text-gray-600 mt-4 italic">
          Studies show significant improvements in working memory after just 19 days of dual n-back training.
        </p>
      </div>
    </div>
  );
};

export default DualNBackGame;