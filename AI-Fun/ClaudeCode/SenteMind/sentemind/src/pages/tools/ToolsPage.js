import React from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  Brain, 
  Heart, 
  PenTool, 
  Wind, 
  TrendingUp, 
  Headphones, 
  ArrowRight,
  Star,
  Lock,
  CheckCircle
} from 'lucide-react';

const ToolsPage = () => {
  const tools = [
    {
      id: 'dual-n-back',
      name: 'Dual N-Back Training',
      description: 'Scientifically proven cognitive training to boost working memory and fluid intelligence',
      icon: Brain,
      color: 'from-purple-500 to-indigo-600',
      difficulty: 'Advanced',
      duration: '15-20 min',
      benefits: ['Working Memory', 'Fluid Intelligence', 'Attention Control'],
      free: true,
      link: '/tools/dual-n-back',
      scientificBacking: 'Multiple peer-reviewed studies show 40% improvement in working memory after 19 days'
    },
    {
      id: 'mindfulness',
      name: 'Mindfulness Assistant',
      description: 'Guided meditation and mindfulness exercises to reduce stress and improve emotional regulation',
      icon: Heart,
      color: 'from-green-500 to-teal-600',
      difficulty: 'Beginner',
      duration: '5-30 min',
      benefits: ['Stress Reduction', 'Emotional Regulation', 'Present Moment Awareness'],
      free: true,
      link: '/tools/mindfulness',
      scientificBacking: 'Harvard studies show 8 weeks of mindfulness increases gray matter in learning areas'
    },
    {
      id: 'journaling',
      name: 'Smart Journaling',
      description: 'AI-powered journaling with personalized prompts and emotional insights',
      icon: PenTool,
      color: 'from-blue-500 to-cyan-600',
      difficulty: 'Beginner',
      duration: '10-15 min',
      benefits: ['Self Awareness', 'Emotional Processing', 'Goal Clarity'],
      free: true,
      link: '/tools/journaling',
      scientificBacking: 'Writing about emotions for 15 minutes improves mental and physical health'
    },
    {
      id: 'breathing',
      name: 'Breath Pacer',
      description: 'Visual breathing exercises to activate your parasympathetic nervous system',
      icon: Wind,
      color: 'from-rose-500 to-pink-600',
      difficulty: 'Beginner',
      duration: '5-10 min',
      benefits: ['Stress Relief', 'Anxiety Reduction', 'Autonomic Balance'],
      free: true,
      link: '/tools/breathing',
      scientificBacking: 'Controlled breathing activates the vagus nerve, reducing cortisol by 25%'
    },
    {
      id: 'mood-tracker',
      name: 'Mood Tracking',
      description: 'Track emotional patterns and identify triggers for better mental health management',
      icon: TrendingUp,
      color: 'from-amber-500 to-orange-600',
      difficulty: 'Beginner',
      duration: '2-5 min',
      benefits: ['Pattern Recognition', 'Trigger Identification', 'Emotional Intelligence'],
      free: false,
      link: '/tools/mood-tracker',
      scientificBacking: 'Daily mood tracking improves emotional regulation and prevents depressive episodes'
    },
    {
      id: 'sound-therapy',
      name: 'Sound Therapy',
      description: 'Binaural beats and healing frequencies for deep relaxation and cognitive enhancement',
      icon: Headphones,
      color: 'from-violet-500 to-purple-600',
      difficulty: 'Intermediate',
      duration: '20-60 min',
      benefits: ['Brainwave Entrainment', 'Deep Relaxation', 'Cognitive Enhancement'],
      free: false,
      link: '/tools/sound-therapy',
      scientificBacking: 'Binaural beats can synchronize brainwaves to enhance focus and relaxation states'
    }
  ];

  const categories = [
    {
      name: 'Cognitive Training',
      description: 'Enhance mental performance and brain function',
      tools: ['dual-n-back'],
      icon: Brain,
      color: 'bg-purple-100 text-purple-700'
    },
    {
      name: 'Stress & Relaxation',
      description: 'Reduce stress and promote mental wellness',
      tools: ['mindfulness', 'breathing', 'sound-therapy'],
      icon: Heart,
      color: 'bg-green-100 text-green-700'
    },
    {
      name: 'Self-Awareness',
      description: 'Develop deeper understanding of your mental patterns',
      tools: ['journaling', 'mood-tracker'],
      icon: TrendingUp,
      color: 'bg-blue-100 text-blue-700'
    }
  ];

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1
      }
    }
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: { opacity: 1, y: 0 }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="tool-container">
        {/* Header */}
        <div className="tool-header">
          <motion.h1 
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            className="tool-title"
          >
            Mental Health Tools
          </motion.h1>
          <motion.p 
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="tool-subtitle"
          >
            Access a comprehensive suite of evidence-based tools designed to enhance your cognitive performance, emotional wellbeing, and mental resilience.
          </motion.p>
        </div>

        {/* Categories Overview */}
        <motion.div
          variants={containerVariants}
          initial="hidden"
          animate="visible"
          className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12"
        >
          {categories.map((category, index) => (
            <motion.div
              key={category.name}
              variants={itemVariants}
              className="card text-center"
            >
              <div className={`w-16 h-16 ${category.color} rounded-full flex items-center justify-center mx-auto mb-4`}>
                <category.icon className="w-8 h-8" />
              </div>
              <h3 className="text-xl font-bold text-gray-800 mb-2">{category.name}</h3>
              <p className="text-gray-600 text-sm mb-4">{category.description}</p>
              <div className="text-sm text-gray-500">
                {category.tools.length} tool{category.tools.length > 1 ? 's' : ''} available
              </div>
            </motion.div>
          ))}
        </motion.div>

        {/* Tools Grid */}
        <motion.div
          variants={containerVariants}
          initial="hidden"
          animate="visible"
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
          {tools.map((tool, index) => (
            <motion.div
              key={tool.id}
              variants={itemVariants}
              className="group"
            >
              <div className="card h-full relative overflow-hidden">
                {/* Premium Badge */}
                {!tool.free && (
                  <div className="absolute top-4 right-4 z-10">
                    <div className="bg-gradient-to-r from-amber-500 to-orange-500 text-white px-2 py-1 rounded-full text-xs font-medium flex items-center">
                      <Star className="w-3 h-3 mr-1" />
                      Premium
                    </div>
                  </div>
                )}

                {/* Tool Icon */}
                <div className={`w-16 h-16 bg-gradient-to-br ${tool.color} rounded-xl flex items-center justify-center mb-6 group-hover:scale-110 transition-transform`}>
                  <tool.icon className="w-8 h-8 text-white" />
                </div>

                {/* Tool Info */}
                <h3 className="text-xl font-bold text-gray-800 mb-3 group-hover:text-primary-600 transition-colors">
                  {tool.name}
                </h3>
                <p className="text-gray-600 mb-4 leading-relaxed">
                  {tool.description}
                </p>

                {/* Tool Details */}
                <div className="space-y-3 mb-6">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-500">Difficulty:</span>
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      tool.difficulty === 'Beginner' ? 'bg-green-100 text-green-700' :
                      tool.difficulty === 'Intermediate' ? 'bg-yellow-100 text-yellow-700' :
                      'bg-red-100 text-red-700'
                    }`}>
                      {tool.difficulty}
                    </span>
                  </div>
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-500">Duration:</span>
                    <span className="text-gray-700 font-medium">{tool.duration}</span>
                  </div>
                </div>

                {/* Benefits */}
                <div className="mb-6">
                  <div className="text-sm font-medium text-gray-700 mb-2">Key Benefits:</div>
                  <div className="flex flex-wrap gap-1">
                    {tool.benefits.map((benefit, benefitIndex) => (
                      <span
                        key={benefitIndex}
                        className="px-2 py-1 bg-gray-100 text-gray-600 rounded text-xs"
                      >
                        {benefit}
                      </span>
                    ))}
                  </div>
                </div>

                {/* Scientific Backing */}
                <div className="mb-6 p-3 bg-blue-50 rounded-lg">
                  <div className="flex items-start space-x-2">
                    <CheckCircle className="w-4 h-4 text-blue-600 mt-0.5 flex-shrink-0" />
                    <div>
                      <div className="text-xs font-medium text-blue-800 mb-1">Scientific Evidence:</div>
                      <div className="text-xs text-blue-700">{tool.scientificBacking}</div>
                    </div>
                  </div>
                </div>

                {/* Action Button */}
                <div className="mt-auto">
                  {tool.free ? (
                    <Link
                      to={tool.link}
                      className="btn btn-primary w-full group-hover:translate-y-[-2px] transition-transform"
                    >
                      Start Training
                      <ArrowRight className="w-4 h-4 ml-2" />
                    </Link>
                  ) : (
                    <div className="space-y-2">
                      <button
                        disabled
                        className="btn bg-gray-200 text-gray-500 w-full cursor-not-allowed"
                      >
                        <Lock className="w-4 h-4 mr-2" />
                        Premium Only
                      </button>
                      <Link
                        to="/premium"
                        className="block text-center text-sm text-primary-600 hover:text-primary-700 font-medium"
                      >
                        Unlock with Premium →
                      </Link>
                    </div>
                  )}
                </div>
              </div>
            </motion.div>
          ))}
        </motion.div>

        {/* Premium CTA */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.8 }}
          className="mt-16 bg-gradient-to-r from-primary-600 to-secondary-600 rounded-2xl p-8 text-center text-white"
        >
          <Star className="w-12 h-12 mx-auto mb-4 text-yellow-300" />
          <h2 className="text-3xl font-bold mb-4">Unlock Your Full Potential</h2>
          <p className="text-xl mb-6 text-primary-100 max-w-2xl mx-auto">
            Get access to all premium tools, advanced analytics, personalized insights, and priority support to accelerate your mental wellness journey.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/premium" className="btn bg-white text-primary-600 hover:bg-gray-100 text-lg px-8 py-4">
              View Premium Plans
            </Link>
            <Link to="/tools/dual-n-back" className="btn border-2 border-white text-white hover:bg-white hover:text-primary-600 text-lg px-8 py-4">
              Try Free Tools First
            </Link>
          </div>
        </motion.div>

        {/* Evidence-Based Approach */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 1 }}
          className="mt-16 bg-white rounded-2xl p-8"
        >
          <div className="text-center mb-8">
            <h2 className="text-3xl font-bold text-gray-800 mb-4">Science-Backed Mental Training</h2>
            <p className="text-lg text-gray-600 max-w-3xl mx-auto">
              Every tool in SenteMind is based on peer-reviewed research from leading institutions like Harvard, Stanford, and MIT. Our approach combines ancient wisdom with modern neuroscience.
            </p>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="text-center">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <CheckCircle className="w-8 h-8 text-green-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-800 mb-2">Clinically Validated</h3>
              <p className="text-gray-600">
                All techniques are supported by clinical trials and peer-reviewed studies
              </p>
            </div>
            
            <div className="text-center">
              <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Brain className="w-8 h-8 text-blue-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-800 mb-2">Neuroscience-Based</h3>
              <p className="text-gray-600">
                Designed using the latest research in neuroplasticity and cognitive science
              </p>
            </div>
            
            <div className="text-center">
              <div className="w-16 h-16 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <TrendingUp className="w-8 h-8 text-purple-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-800 mb-2">Measurable Results</h3>
              <p className="text-gray-600">
                Track your progress with quantifiable improvements in mental performance
              </p>
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
};

export default ToolsPage;