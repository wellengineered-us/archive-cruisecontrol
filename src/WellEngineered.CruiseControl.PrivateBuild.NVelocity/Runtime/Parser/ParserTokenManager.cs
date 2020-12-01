/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied.  See the License for the
* specific language governing permissions and limitations
* under the License.    
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser
{
	/// <summary>
    /// 
    /// </summary>
    public class ParserTokenManager : ParserConstants
    {
        private void InitBlock()
        {
            this.debugStream = Console.Out;
        }
  
        public TextWriter DebugStream
        {
            set
            {
                this.debugStream = value;
            }

        }
        public Token NextToken
        {
            get
            {
                int kind;
                Token specialToken = null;
                Token matchedToken;
                int curPos = 0;

                for (; ; )
                {
                    try
                    {
                        this.curChar = this.input_stream.BeginToken();
                    }
                    catch (IOException e)
                    {
                        this.jjmatchedKind = 0;
                        matchedToken = this.jjFillToken();
                        matchedToken.SpecialToken = specialToken;
                        return matchedToken;
                    }
                    this.image = null;
                    this.jjimageLen = 0;

                    for (; ; )
                    {
                        switch (this.curLexState)
                        {

                            case 0:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_0();
                                break;

                            case 1:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_1();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 66)
                                {
                                    this.jjmatchedKind = 66;
                                }
                                break;

                            case 2:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_2();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 66)
                                {
                                    this.jjmatchedKind = 66;
                                }
                                break;

                            case 3:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_3();
                                break;

                            case 4:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_4();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 66)
                                {
                                    this.jjmatchedKind = 66;
                                }
                                break;

                            case 5:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_5();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 67)
                                {
                                    this.jjmatchedKind = 67;
                                }
                                break;

                            case 6:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_6();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 25)
                                {
                                    this.jjmatchedKind = 25;
                                }
                                break;

                            case 7:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_7();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 25)
                                {
                                    this.jjmatchedKind = 25;
                                }
                                break;

                            case 8:
                                this.jjmatchedKind = 0x7fffffff;
                                this.jjmatchedPos = 0;
                                curPos = this.jjMoveStringLiteralDfa0_8();
                                if (this.jjmatchedPos == 0 && this.jjmatchedKind > 25)
                                {
                                    this.jjmatchedKind = 25;
                                }
                                break;
                        }
                        if (this.jjmatchedKind != 0x7fffffff)
                        {
                            if (this.jjmatchedPos + 1 < curPos)
                                this.input_stream.Backup(curPos - this.jjmatchedPos - 1);
                            if ((jjtoToken[this.jjmatchedKind >> 6] & (ulong)(1L << (this.jjmatchedKind & 63))) != 0L)
                            {
                                matchedToken = this.jjFillToken();
                                matchedToken.SpecialToken = specialToken;
                                this.TokenLexicalActions(matchedToken);
                                if (jjnewLexState[this.jjmatchedKind] != -1)
                                    this.curLexState = jjnewLexState[this.jjmatchedKind];
                                return matchedToken;
                            }
                            else if ((jjtoSkip[this.jjmatchedKind >> 6] & (1L << (this.jjmatchedKind & 63))) != 0L)
                            {
                                if ((jjtoSpecial[this.jjmatchedKind >> 6] & (1L << (this.jjmatchedKind & 63))) != 0L)
                                {
                                    matchedToken = this.jjFillToken();
                                    if (specialToken == null)
                                        specialToken = matchedToken;
                                    else
                                    {
                                        matchedToken.SpecialToken = specialToken;
                                        specialToken = (specialToken.Next = matchedToken);
                                    }
                                    this.SkipLexicalActions(matchedToken);
                                }
                                else
                                    this.SkipLexicalActions(null);
                                if (jjnewLexState[this.jjmatchedKind] != -1)
                                    this.curLexState = jjnewLexState[this.jjmatchedKind];
                             
                                goto EOFLoop;
                            }
                            this.MoreLexicalActions();
                            if (jjnewLexState[this.jjmatchedKind] != -1)
                                this.curLexState = jjnewLexState[this.jjmatchedKind];
                            curPos = 0;
                            this.jjmatchedKind = 0x7fffffff;
                            try
                            {
                                this.curChar = this.input_stream.ReadChar();
                                continue;
                            }
                            catch (System.IO.IOException e1)
                            {
                            }
                        }
                        int error_line = this.input_stream.EndLine;
                        int error_column = this.input_stream.EndColumn;
                        System.String error_after = null;
                        bool EOFSeen = false;
                        try
                        {
                            this.input_stream.ReadChar(); this.input_stream.Backup(1);
                        }
                        catch (System.IO.IOException e1)
                        {
                            EOFSeen = true;
                            error_after = curPos <= 1 ? "" : this.input_stream.GetImage();
                            if (this.curChar == '\n' || this.curChar == '\r')
                            {
                                error_line++;
                                error_column = 0;
                            }
                            else
                                error_column++;
                        }
                        if (!EOFSeen)
                        {
                            this.input_stream.Backup(1);
                            error_after = curPos <= 1 ? "" : this.input_stream.GetImage();
                        }
                        throw new TokenMgrError(EOFSeen, this.curLexState, error_line, error_column, error_after, this.curChar, TokenMgrError.LEXICAL_ERROR);
                    }

                EOFLoop: ;
                }
            }

        }
        private int fileDepth = 0;

        private int lparen = 0;
        private int rparen = 0;

        internal Stack<Dictionary<string, int>> stateStack = new Stack<Dictionary<string, int>>();
        public bool debugPrint = false;

        private bool inReference;
        public bool inDirective;
        private bool inComment;
        public bool inSet;

        /// <summary>  pushes the current state onto the 'state stack',
        /// and maintains the parens counts
        /// public because we need it in PD & VM handling
        /// 
        /// </summary>
        /// <returns> boolean : success.  It can fail if the state machine
        /// gets messed up (do don't mess it up :)
        /// </returns>
        public virtual bool stateStackPop()
        {
            Dictionary<string, int> h;

            try
            {
                h = this.stateStack.Pop();
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                this.lparen = 0;
                this.SwitchTo(ParserConstants.DEFAULT);
                return false;
            }

            if (this.debugPrint)
                System.Console.Out.WriteLine(" stack pop (" + this.stateStack.Count + ") : lparen=" + ((System.Int32)h["lparen"]) + " newstate=" + ((System.Int32)h["lexstate"]));

            this.lparen = ((System.Int32)h["lparen"]);
            this.rparen = ((System.Int32)h["rparen"]);

            this.SwitchTo(((System.Int32)h["lexstate"]));

            return true;
        }

        /// <summary>  pops a state off the stack, and restores paren counts
        /// 
        /// </summary>
        /// <returns> boolean : success of operation
        /// </returns>
        public virtual bool stateStackPush()
        {
            if (this.debugPrint)
                System.Console.Out.WriteLine(" (" + this.stateStack.Count + ") pushing cur state : " + this.curLexState);

            Dictionary<string, int> h = new Dictionary<string, int>();

            h["lexstate"] = this.curLexState;
            h["lparen"] = this.lparen;
            h["rparen"] = this.rparen;

            this.lparen = 0;

            this.stateStack.Push(h);

            return true;
        }

        /// <summary>  Clears all state variables, resets to
        /// start values, clears stateStack.  Call
        /// before parsing.
        /// </summary>
        /// <returns> void
        /// </returns>
        public virtual void clearStateVars()
        {
            this.stateStack.Clear();

            this.lparen = 0;
            this.rparen = 0;
            this.inReference = false;
            this.inDirective = false;
            this.inComment = false;
            this.inSet = false;

            return;
        }

        /// <summary>  handles the dropdown logic when encountering a RPAREN</summary>
        private void RPARENHandler()
        {
            /*
            *  Ultimately, we want to drop down to the state below
            *  the one that has an open (if we hit bottom (DEFAULT),
            *  that's fine. It's just text schmoo.
            */

            bool closed = false;

            if (this.inComment)
                closed = true;

            while (!closed)
            {
                /*
                * look at current state.  If we haven't seen a lparen
                * in this state then we drop a state, because this
                * lparen clearly closes our state
                */

                if (this.lparen > 0)
                {
                    /*
                    *  if rparen + 1 == lparen, then this state is closed.
                    * Otherwise, increment and keep parsing
                    */

                    if (this.lparen == this.rparen + 1)
                    {
                        this.stateStackPop();
                    }
                    else
                    {
                        this.rparen++;
                    }

                    closed = true;
                }
                else
                {
                    /*
                    * now, drop a state
                    */

                    if (!this.stateStackPop())
                        break;
                }
            }
        }
    
        public TextWriter debugStream;
        private int jjStopStringLiteralDfa_0(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x10L) != 0L)
                        return 58;
                    if ((active0 & 0x80000000L) != 0L)
                        return 101;
                    if ((active0 & 0x40L) != 0L)
                        return 65;
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 57;
                        return 63;
                    }
                    if ((active0 & 0x200000000000L) != 0L)
                        return 50;
                    if ((active0 & 0x70000L) != 0L)
                        return 7;
                    return -1;

                case 1:
                    if ((active0 & 0x10000L) != 0L)
                        return 5;
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 57;
                        this.jjmatchedPos = 1;
                        return 63;
                    }
                    return -1;

                case 2:
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 57;
                        this.jjmatchedPos = 2;
                        return 63;
                    }
                    return -1;

                case 3:
                    if ((active0 & 0x10000000L) != 0L)
                        return 63;
                    if ((active0 & 0x20000000L) != 0L)
                    {
                        this.jjmatchedKind = 57;
                        this.jjmatchedPos = 3;
                        return 63;
                    }
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_0(int pos, long active0)
        {
            return this.jjMoveNfa_0(this.jjStopStringLiteralDfa_0(pos, active0), pos + 1);
        }
        private int jjStopAtPos(int pos, int kind)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            return pos + 1;
        }
        private int jjStartNfaWithStates_0(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_0(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_0()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_0(0x50000L);

                case (char)(37):
                    return this.jjStopAtPos(0, 35);

                case (char)(40):
                    return this.jjStopAtPos(0, 8);

                case (char)(42):
                    return this.jjStopAtPos(0, 33);

                case (char)(43):
                    return this.jjStopAtPos(0, 32);

                case (char)(44):
                    return this.jjStopAtPos(0, 3);

                case (char)(45):
                    return this.jjStartNfaWithStates_0(0, 31, 101);

                case (char)(46):
                    return this.jjMoveStringLiteralDfa1_0(0x10L);

                case (char)(47):
                    return this.jjStopAtPos(0, 34);

                case (char)(58):
                    return this.jjStopAtPos(0, 5);

                case (char)(61):
                    return this.jjStartNfaWithStates_0(0, 45, 50);

                case (char)(91):
                    return this.jjStopAtPos(0, 1);

                case (char)(93):
                    return this.jjStopAtPos(0, 2);

                case (char)(102):
                    return this.jjMoveStringLiteralDfa1_0(0x20000000L);

                case (char)(116):
                    return this.jjMoveStringLiteralDfa1_0(0x10000000L);

                case (char)(123):
                    return this.jjStartNfaWithStates_0(0, 6, 65);

                case (char)(125):
                    return this.jjStopAtPos(0, 7);

                default:
                    return this.jjMoveNfa_0(0, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_0(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_0(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_0(1, 16, 5);
                    break;

                case (char)(46):
                    if ((active0 & 0x10L) != 0L)
                        return this.jjStopAtPos(1, 4);
                    break;

                case (char)(97):
                    return this.jjMoveStringLiteralDfa2_0(active0, 0x20000000L);

                case (char)(114):
                    return this.jjMoveStringLiteralDfa2_0(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_0(0, active0);
        }
        private int jjMoveStringLiteralDfa2_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_0(0, old0);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_0(1, active0);
                return 2;
            }
            switch (this.curChar)
            {

                case (char)(108):
                    return this.jjMoveStringLiteralDfa3_0(active0, 0x20000000L);

                case (char)(117):
                    return this.jjMoveStringLiteralDfa3_0(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_0(1, active0);
        }
        private int jjMoveStringLiteralDfa3_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_0(1, old0);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_0(2, active0);
                return 3;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x10000000L) != 0L)
                        return this.jjStartNfaWithStates_0(3, 28, 63);
                    break;

                case (char)(115):
                    return this.jjMoveStringLiteralDfa4_0(active0, 0x20000000L);

                default:
                    break;

            }
            return this.jjStartNfa_0(2, active0);
        }
        private int jjMoveStringLiteralDfa4_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_0(2, old0);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_0(3, active0);
                return 4;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x20000000L) != 0L)
                        return this.jjStartNfaWithStates_0(4, 29, 63);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_0(3, active0);
        }
        private void jjCheckNAdd(int state)
        {
            if (this.jjrounds[state] != this.jjround)
            {
                this.jjstateSet[this.jjnewStateCnt++] = (uint)state;
                this.jjrounds[state] = this.jjround;
            }
        }
        private void jjAddStates(int start, int end)
        {
            do
            {
                this.jjstateSet[this.jjnewStateCnt++] = (uint)jjnextStates[start];
            }
            while (start++ != end);
        }
        private void jjCheckNAddTwoStates(int state1, int state2)
        {
            this.jjCheckNAdd(state1);
            this.jjCheckNAdd(state2);
        }
        private void jjCheckNAddStates(int start, int end)
        {
            do
            {
                this.jjCheckNAdd(jjnextStates[start]);
            }
            while (start++ != end);
        }
        private void jjCheckNAddStates(int start)
        {
            this.jjCheckNAdd(jjnextStates[start]);
            this.jjCheckNAdd(jjnextStates[start + 1]);
        }
        internal static readonly ulong[] jjbitVec0 = { 0xfffffffffffffffeL, 0xffffffffffffffffL, 0xffffffffffffffffL, 0xffffffffffffffffL };
        internal static readonly ulong[] jjbitVec2 = { 0x0L, 0x0L, 0xffffffffffffffffL, 0xffffffffffffffffL };
        private int jjMoveNfa_0(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 101;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 0:
                                if ((0x3ff000000000000L & l) != 0L)
                                {
                                    if (kind > 52)
                                        kind = 52;
                                    this.jjCheckNAddStates(0, 5);
                                }
                                else if ((0x100002600L & l) != 0L)
                                {
                                    if (kind > 26)
                                        kind = 26;
                                    this.jjCheckNAdd(9);
                                }
                                else if (this.curChar == 45)
                                    this.jjCheckNAddStates(6, 9);
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(73, 74);
                                }
                                else if (this.curChar == 46)
                                    this.jjCheckNAdd(58);
                                else if (this.curChar == 33)
                                {
                                    if (kind > 44)
                                        kind = 44;
                                }
                                else if (this.curChar == 61)
                                    this.jjstateSet[this.jjnewStateCnt++] = 50;
                                else if (this.curChar == 62)
                                    this.jjstateSet[this.jjnewStateCnt++] = 48;
                                else if (this.curChar == 60)
                                    this.jjstateSet[this.jjnewStateCnt++] = 45;
                                else if (this.curChar == 38)
                                    this.jjstateSet[this.jjnewStateCnt++] = 35;
                                else if (this.curChar == 39)
                                    this.jjCheckNAddStates(10, 12);
                                else if (this.curChar == 34)
                                    this.jjCheckNAddStates(13, 15);
                                else if (this.curChar == 35)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                else if (this.curChar == 41)
                                {
                                    if (kind > 9)
                                        kind = 9;
                                    this.jjCheckNAddStates(16, 18);
                                }
                                if ((0x2400L & l) != 0L)
                                {
                                    if (kind > 30)
                                        kind = 30;
                                }
                                else if (this.curChar == 33)
                                    this.jjstateSet[this.jjnewStateCnt++] = 54;
                                else if (this.curChar == 62)
                                {
                                    if (kind > 40)
                                        kind = 40;
                                }
                                else if (this.curChar == 60)
                                {
                                    if (kind > 38)
                                        kind = 38;
                                }
                                if (this.curChar == 13)
                                    this.jjstateSet[this.jjnewStateCnt++] = 33;
                                break;

                            case 101:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(96, 97);
                                else if (this.curChar == 46)
                                    this.jjCheckNAdd(58);
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(90, 91);
                                if ((0x3ff000000000000L & l) != 0L)
                                {
                                    if (kind > 52)
                                        kind = 52;
                                    this.jjCheckNAddTwoStates(87, 89);
                                }
                                break;

                            case 1:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddStates(16, 18);
                                break;

                            case 2:
                                if ((0x2400L & l) != 0L && kind > 9)
                                    kind = 9;
                                break;

                            case 3:
                                if (this.curChar == 10 && kind > 9)
                                    kind = 9;
                                break;

                            case 4:
                                if (this.curChar == 13)
                                    this.jjstateSet[this.jjnewStateCnt++] = 3;
                                break;

                            case 5:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 6;
                                break;

                            case 6:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            case 7:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 8:
                                if (this.curChar == 35)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                break;

                            case 9:
                                if ((0x100002600L & l) == 0L)
                                    break;
                                if (kind > 26)
                                    kind = 26;
                                this.jjCheckNAdd(9);
                                break;

                            case 10:
                                if (this.curChar == 34)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 11:
                                if ((0xfffffffbffffffffUL & (ulong)l) != 0L)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 12:
                                if (this.curChar == 34 && kind > 27)
                                    kind = 27;
                                break;

                            case 14:
                                if ((0x8400000000L & l) != 0L)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 15:
                                if ((0xff000000000000L & l) != 0L)
                                    this.jjCheckNAddStates(19, 22);
                                break;

                            case 16:
                                if ((0xff000000000000L & l) != 0L)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 17:
                                if ((0xf000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 18;
                                break;

                            case 18:
                                if ((0xff000000000000L & l) != 0L)
                                    this.jjCheckNAdd(16);
                                break;

                            case 20:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 21;
                                break;

                            case 21:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 22;
                                break;

                            case 22:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 23;
                                break;

                            case 23:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 24:
                                if (this.curChar == 32)
                                    this.jjAddStates(23, 24);
                                break;

                            case 25:
                                if (this.curChar == 10)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 26:
                                if (this.curChar == 39)
                                    this.jjCheckNAddStates(10, 12);
                                break;

                            case 27:
                                if ((0xffffff7fffffffffUL & (ulong)l) != 0L)
                                    this.jjCheckNAddStates(10, 12);
                                break;

                            case 29:
                                if (this.curChar == 32)
                                    this.jjAddStates(25, 26);
                                break;

                            case 30:
                                if (this.curChar == 10)
                                    this.jjCheckNAddStates(10, 12);
                                break;

                            case 31:
                                if (this.curChar == 39 && kind > 27)
                                    kind = 27;
                                break;

                            case 32:
                                if ((0x2400L & l) != 0L && kind > 30)
                                    kind = 30;
                                break;

                            case 33:
                                if (this.curChar == 10 && kind > 30)
                                    kind = 30;
                                break;

                            case 34:
                                if (this.curChar == 13)
                                    this.jjstateSet[this.jjnewStateCnt++] = 33;
                                break;

                            case 35:
                                if (this.curChar == 38 && kind > 36)
                                    kind = 36;
                                break;

                            case 36:
                                if (this.curChar == 38)
                                    this.jjstateSet[this.jjnewStateCnt++] = 35;
                                break;

                            case 44:
                                if (this.curChar == 60 && kind > 38)
                                    kind = 38;
                                break;

                            case 45:
                                if (this.curChar == 61 && kind > 39)
                                    kind = 39;
                                break;

                            case 46:
                                if (this.curChar == 60)
                                    this.jjstateSet[this.jjnewStateCnt++] = 45;
                                break;

                            case 47:
                                if (this.curChar == 62 && kind > 40)
                                    kind = 40;
                                break;

                            case 48:
                                if (this.curChar == 61 && kind > 41)
                                    kind = 41;
                                break;

                            case 49:
                                if (this.curChar == 62)
                                    this.jjstateSet[this.jjnewStateCnt++] = 48;
                                break;

                            case 50:
                                if (this.curChar == 61 && kind > 42)
                                    kind = 42;
                                break;

                            case 51:
                                if (this.curChar == 61)
                                    this.jjstateSet[this.jjnewStateCnt++] = 50;
                                break;

                            case 54:
                                if (this.curChar == 61 && kind > 43)
                                    kind = 43;
                                break;

                            case 55:
                                if (this.curChar == 33)
                                    this.jjstateSet[this.jjnewStateCnt++] = 54;
                                break;

                            case 56:
                                if (this.curChar == 33 && kind > 44)
                                    kind = 44;
                                break;

                            case 57:
                                if (this.curChar == 46)
                                    this.jjCheckNAdd(58);
                                break;

                            case 58:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(58, 59);
                                break;

                            case 60:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(61);
                                break;

                            case 61:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(61);
                                break;

                            case 63:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 57)
                                    kind = 57;
                                this.jjstateSet[this.jjnewStateCnt++] = 63;
                                break;

                            case 66:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjAddStates(27, 28);
                                break;

                            case 70:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 72:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(73, 74);
                                break;

                            case 74:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 75:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(73, 74);
                                break;

                            case 86:
                                if (this.curChar == 45)
                                    this.jjCheckNAddStates(6, 9);
                                break;

                            case 87:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 52)
                                    kind = 52;
                                this.jjCheckNAddTwoStates(87, 89);
                                break;

                            case 88:
                                if (this.curChar == 46 && kind > 52)
                                    kind = 52;
                                break;

                            case 89:
                                if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 88;
                                break;

                            case 90:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(90, 91);
                                break;

                            case 91:
                                if (this.curChar != 46)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(92, 93);
                                break;

                            case 92:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(92, 93);
                                break;

                            case 94:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(95);
                                break;

                            case 95:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(95);
                                break;

                            case 96:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(96, 97);
                                break;

                            case 98:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(99);
                                break;

                            case 99:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(99);
                                break;

                            case 100:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 52)
                                    kind = 52;
                                this.jjCheckNAddStates(0, 5);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 0:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                {
                                    if (kind > 57)
                                        kind = 57;
                                    this.jjCheckNAdd(63);
                                }
                                else if (this.curChar == 92)
                                    this.jjCheckNAddStates(29, 32);
                                else if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 65;
                                else if (this.curChar == 124)
                                    this.jjstateSet[this.jjnewStateCnt++] = 40;
                                if (this.curChar == 110)
                                    this.jjAddStates(33, 34);
                                else if (this.curChar == 103)
                                    this.jjAddStates(35, 36);
                                else if (this.curChar == 108)
                                    this.jjAddStates(37, 38);
                                else if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 52;
                                else if (this.curChar == 111)
                                    this.jjstateSet[this.jjnewStateCnt++] = 42;
                                else if (this.curChar == 97)
                                    this.jjstateSet[this.jjnewStateCnt++] = 38;
                                break;

                            case 6:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            case 11:
                                this.jjCheckNAddStates(13, 15);
                                break;

                            case 13:
                                if (this.curChar == 92)
                                    this.jjAddStates(39, 44);
                                break;

                            case 14:
                                if ((0x14404410000000L & l) != 0L)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 19:
                                if (this.curChar == 117)
                                    this.jjstateSet[this.jjnewStateCnt++] = 20;
                                break;

                            case 20:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 21;
                                break;

                            case 21:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 22;
                                break;

                            case 22:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 23;
                                break;

                            case 23:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjCheckNAddStates(13, 15);
                                break;

                            case 27:
                                this.jjAddStates(10, 12);
                                break;

                            case 28:
                                if (this.curChar == 92)
                                    this.jjAddStates(25, 26);
                                break;

                            case 37:
                                if (this.curChar == 100 && kind > 36)
                                    kind = 36;
                                break;

                            case 38:
                                if (this.curChar == 110)
                                    this.jjstateSet[this.jjnewStateCnt++] = 37;
                                break;

                            case 39:
                                if (this.curChar == 97)
                                    this.jjstateSet[this.jjnewStateCnt++] = 38;
                                break;

                            case 40:
                                if (this.curChar == 124 && kind > 37)
                                    kind = 37;
                                break;

                            case 41:
                                if (this.curChar == 124)
                                    this.jjstateSet[this.jjnewStateCnt++] = 40;
                                break;

                            case 42:
                                if (this.curChar == 114 && kind > 37)
                                    kind = 37;
                                break;

                            case 43:
                                if (this.curChar == 111)
                                    this.jjstateSet[this.jjnewStateCnt++] = 42;
                                break;

                            case 52:
                                if (this.curChar == 113 && kind > 42)
                                    kind = 42;
                                break;

                            case 53:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 52;
                                break;

                            case 59:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(45, 46);
                                break;

                            case 62:
                            case 63:
                                if ((0x7fffffe87fffffeL & l) == 0L)
                                    break;
                                if (kind > 57)
                                    kind = 57;
                                this.jjCheckNAdd(63);
                                break;

                            case 64:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 65;
                                break;

                            case 65:
                            case 66:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                    this.jjCheckNAddTwoStates(66, 67);
                                break;

                            case 67:
                                if (this.curChar == 125 && kind > 58)
                                    kind = 58;
                                break;

                            case 68:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(29, 32);
                                break;

                            case 69:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(69, 70);
                                break;

                            case 71:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(71, 72);
                                break;

                            case 73:
                                if (this.curChar == 92)
                                    this.jjAddStates(47, 48);
                                break;

                            case 76:
                                if (this.curChar == 108)
                                    this.jjAddStates(37, 38);
                                break;

                            case 77:
                                if (this.curChar == 116 && kind > 38)
                                    kind = 38;
                                break;

                            case 78:
                                if (this.curChar == 101 && kind > 39)
                                    kind = 39;
                                break;

                            case 79:
                                if (this.curChar == 103)
                                    this.jjAddStates(35, 36);
                                break;

                            case 80:
                                if (this.curChar == 116 && kind > 40)
                                    kind = 40;
                                break;

                            case 81:
                                if (this.curChar == 101 && kind > 41)
                                    kind = 41;
                                break;

                            case 82:
                                if (this.curChar == 110)
                                    this.jjAddStates(33, 34);
                                break;

                            case 83:
                                if (this.curChar == 101 && kind > 43)
                                    kind = 43;
                                break;

                            case 84:
                                if (this.curChar == 116 && kind > 44)
                                    kind = 44;
                                break;

                            case 85:
                                if (this.curChar == 111)
                                    this.jjstateSet[this.jjnewStateCnt++] = 84;
                                break;

                            case 93:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(49, 50);
                                break;

                            case 97:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(51, 52);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 6:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            case 11:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    this.jjAddStates(13, 15);
                                break;

                            case 27:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    this.jjAddStates(10, 12);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 101 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_6(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 2;
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_6(int pos, long active0)
        {
            return this.jjMoveNfa_6(this.jjStopStringLiteralDfa_6(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_6(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_6(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_6()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_6(0x50000L);

                case (char)(42):
                    return this.jjMoveStringLiteralDfa1_6(0x1000000L);

                default:
                    return this.jjMoveNfa_6(3, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_6(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_6(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    else if ((active0 & 0x1000000L) != 0L)
                        return this.jjStopAtPos(1, 24);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_6(1, 16, 0);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_6(0, active0);
        }
        private int jjMoveNfa_6(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 12;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(9, 10);
                                }
                                else if (this.curChar == 35)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 0:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 1;
                                break;

                            case 1:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            case 2:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 0;
                                break;

                            case 6:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 8:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(9, 10);
                                break;

                            case 10:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 11:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(9, 10);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(53, 56);
                                break;

                            case 1:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            case 5:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(5, 6);
                                break;

                            case 7:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(7, 8);
                                break;

                            case 9:
                                if (this.curChar == 92)
                                    this.jjAddStates(57, 58);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 1:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 12 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_5(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 2;
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_5(int pos, long active0)
        {
            return this.jjMoveNfa_5(this.jjStopStringLiteralDfa_5(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_5(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_5(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_5()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_5(0x50000L);

                default:
                    return this.jjMoveNfa_5(3, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_5(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_5(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_5(1, 16, 0);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_5(0, active0);
        }
        private int jjMoveNfa_5(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 92;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if ((0x3ff000000000000L & l) != 0L)
                                {
                                    if (kind > 52)
                                        kind = 52;
                                    this.jjCheckNAddStates(59, 64);
                                }
                                else if (this.curChar == 45)
                                    this.jjCheckNAddStates(65, 68);
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(26, 27);
                                }
                                else if (this.curChar == 46)
                                    this.jjCheckNAdd(11);
                                else if (this.curChar == 35)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 0:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 1;
                                break;

                            case 1:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            case 2:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 0;
                                break;

                            case 10:
                                if (this.curChar == 46)
                                    this.jjCheckNAdd(11);
                                break;

                            case 11:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(11, 12);
                                break;

                            case 13:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(14);
                                break;

                            case 14:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(14);
                                break;

                            case 16:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 57)
                                    kind = 57;
                                this.jjstateSet[this.jjnewStateCnt++] = 16;
                                break;

                            case 19:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjAddStates(69, 70);
                                break;

                            case 23:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 25:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(26, 27);
                                break;

                            case 27:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 28:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(26, 27);
                                break;

                            case 31:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddStates(71, 73);
                                break;

                            case 32:
                                if ((0x2400L & l) != 0L && kind > 46)
                                    kind = 46;
                                break;

                            case 33:
                                if (this.curChar == 10 && kind > 46)
                                    kind = 46;
                                break;

                            case 34:
                            case 51:
                                if (this.curChar == 13)
                                    this.jjCheckNAdd(33);
                                break;

                            case 42:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddStates(74, 76);
                                break;

                            case 43:
                                if ((0x2400L & l) != 0L && kind > 49)
                                    kind = 49;
                                break;

                            case 44:
                                if (this.curChar == 10 && kind > 49)
                                    kind = 49;
                                break;

                            case 45:
                            case 67:
                                if (this.curChar == 13)
                                    this.jjCheckNAdd(44);
                                break;

                            case 50:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddStates(77, 79);
                                break;

                            case 66:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddStates(80, 82);
                                break;

                            case 77:
                                if (this.curChar == 45)
                                    this.jjCheckNAddStates(65, 68);
                                break;

                            case 78:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 52)
                                    kind = 52;
                                this.jjCheckNAddTwoStates(78, 80);
                                break;

                            case 79:
                                if (this.curChar == 46 && kind > 52)
                                    kind = 52;
                                break;

                            case 80:
                                if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 79;
                                break;

                            case 81:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(81, 82);
                                break;

                            case 82:
                                if (this.curChar != 46)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(83, 84);
                                break;

                            case 83:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(83, 84);
                                break;

                            case 85:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(86);
                                break;

                            case 86:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(86);
                                break;

                            case 87:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(87, 88);
                                break;

                            case 89:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(90);
                                break;

                            case 90:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(90);
                                break;

                            case 91:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 52)
                                    kind = 52;
                                this.jjCheckNAddStates(59, 64);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                {
                                    if (kind > 57)
                                        kind = 57;
                                    this.jjCheckNAdd(16);
                                }
                                else if (this.curChar == 123)
                                    this.jjAddStates(83, 87);
                                else if (this.curChar == 92)
                                    this.jjCheckNAddStates(88, 91);
                                if (this.curChar == 101)
                                    this.jjAddStates(92, 94);
                                else if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 18;
                                else if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 8;
                                else if (this.curChar == 105)
                                    this.jjstateSet[this.jjnewStateCnt++] = 4;
                                break;

                            case 1:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            case 4:
                                if (this.curChar == 102 && kind > 47)
                                    kind = 47;
                                break;

                            case 5:
                                if (this.curChar == 105)
                                    this.jjstateSet[this.jjnewStateCnt++] = 4;
                                break;

                            case 6:
                                if (this.curChar == 112 && kind > 50)
                                    kind = 50;
                                break;

                            case 7:
                                if (this.curChar == 111)
                                    this.jjstateSet[this.jjnewStateCnt++] = 6;
                                break;

                            case 8:
                                if (this.curChar == 116)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                break;

                            case 9:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 8;
                                break;

                            case 12:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(95, 96);
                                break;

                            case 15:
                            case 16:
                                if ((0x7fffffe87fffffeL & l) == 0L)
                                    break;
                                if (kind > 57)
                                    kind = 57;
                                this.jjCheckNAdd(16);
                                break;

                            case 17:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 18;
                                break;

                            case 18:
                            case 19:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                    this.jjCheckNAddTwoStates(19, 20);
                                break;

                            case 20:
                                if (this.curChar == 125 && kind > 58)
                                    kind = 58;
                                break;

                            case 21:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(88, 91);
                                break;

                            case 22:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(22, 23);
                                break;

                            case 24:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(24, 25);
                                break;

                            case 26:
                                if (this.curChar == 92)
                                    this.jjAddStates(97, 98);
                                break;

                            case 29:
                                if (this.curChar == 101)
                                    this.jjAddStates(92, 94);
                                break;

                            case 30:
                                if (this.curChar != 100)
                                    break;
                                if (kind > 46)
                                    kind = 46;
                                this.jjCheckNAddStates(71, 73);
                                break;

                            case 35:
                                if (this.curChar == 110)
                                    this.jjstateSet[this.jjnewStateCnt++] = 30;
                                break;

                            case 36:
                                if (this.curChar == 102 && kind > 48)
                                    kind = 48;
                                break;

                            case 37:
                                if (this.curChar == 105)
                                    this.jjstateSet[this.jjnewStateCnt++] = 36;
                                break;

                            case 38:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 37;
                                break;

                            case 39:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 38;
                                break;

                            case 40:
                                if (this.curChar == 108)
                                    this.jjstateSet[this.jjnewStateCnt++] = 39;
                                break;

                            case 41:
                                if (this.curChar != 101)
                                    break;
                                if (kind > 49)
                                    kind = 49;
                                this.jjCheckNAddStates(74, 76);
                                break;

                            case 46:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 41;
                                break;

                            case 47:
                                if (this.curChar == 108)
                                    this.jjstateSet[this.jjnewStateCnt++] = 46;
                                break;

                            case 48:
                                if (this.curChar == 123)
                                    this.jjAddStates(83, 87);
                                break;

                            case 49:
                                if (this.curChar != 125)
                                    break;
                                if (kind > 46)
                                    kind = 46;
                                this.jjCheckNAddStates(77, 79);
                                break;

                            case 52:
                                if (this.curChar == 100)
                                    this.jjstateSet[this.jjnewStateCnt++] = 49;
                                break;

                            case 53:
                                if (this.curChar == 110)
                                    this.jjstateSet[this.jjnewStateCnt++] = 52;
                                break;

                            case 54:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 53;
                                break;

                            case 55:
                                if (this.curChar == 125 && kind > 47)
                                    kind = 47;
                                break;

                            case 56:
                                if (this.curChar == 102)
                                    this.jjstateSet[this.jjnewStateCnt++] = 55;
                                break;

                            case 57:
                                if (this.curChar == 105)
                                    this.jjstateSet[this.jjnewStateCnt++] = 56;
                                break;

                            case 58:
                                if (this.curChar == 125 && kind > 48)
                                    kind = 48;
                                break;

                            case 59:
                                if (this.curChar == 102)
                                    this.jjstateSet[this.jjnewStateCnt++] = 58;
                                break;

                            case 60:
                                if (this.curChar == 105)
                                    this.jjstateSet[this.jjnewStateCnt++] = 59;
                                break;

                            case 61:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 60;
                                break;

                            case 62:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 61;
                                break;

                            case 63:
                                if (this.curChar == 108)
                                    this.jjstateSet[this.jjnewStateCnt++] = 62;
                                break;

                            case 64:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 63;
                                break;

                            case 65:
                                if (this.curChar != 125)
                                    break;
                                if (kind > 49)
                                    kind = 49;
                                this.jjCheckNAddStates(80, 82);
                                break;

                            case 68:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 65;
                                break;

                            case 69:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 68;
                                break;

                            case 70:
                                if (this.curChar == 108)
                                    this.jjstateSet[this.jjnewStateCnt++] = 69;
                                break;

                            case 71:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 70;
                                break;

                            case 72:
                                if (this.curChar == 125 && kind > 50)
                                    kind = 50;
                                break;

                            case 73:
                                if (this.curChar == 112)
                                    this.jjstateSet[this.jjnewStateCnt++] = 72;
                                break;

                            case 74:
                                if (this.curChar == 111)
                                    this.jjstateSet[this.jjnewStateCnt++] = 73;
                                break;

                            case 75:
                                if (this.curChar == 116)
                                    this.jjstateSet[this.jjnewStateCnt++] = 74;
                                break;

                            case 76:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 75;
                                break;

                            case 84:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(99, 100);
                                break;

                            case 88:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(101, 102);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 1:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 92 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_3(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x180000L) != 0L)
                        return 14;
                    if ((active0 & 0x70000L) != 0L)
                        return 33;
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_3(int pos, long active0)
        {
            return this.jjMoveNfa_3(this.jjStopStringLiteralDfa_3(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_3(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_3(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_3()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_3(0x50000L);

                case (char)(92):
                    this.jjmatchedKind = 20;
                    return this.jjMoveStringLiteralDfa1_3(0x80000L);

                default:
                    return this.jjMoveNfa_3(22, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_3(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_3(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_3(1, 16, 31);
                    break;

                case (char)(92):
                    if ((active0 & 0x80000L) != 0L)
                        return this.jjStartNfaWithStates_3(1, 19, 34);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_3(0, active0);
        }
        private int jjMoveNfa_3(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 34;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 22:
                                if ((0xffffffe7ffffffffUL & (ulong)l) != 0L)
                                {
                                    if (kind > 21)
                                        kind = 21;
                                    this.jjCheckNAdd(12);
                                }
                                else if (this.curChar == 35)
                                    this.jjCheckNAddStates(103, 105);
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(27, 28);
                                }
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                break;

                            case 14:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(27, 28);
                                else if (this.curChar == 35)
                                    this.jjAddStates(106, 107);
                                if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                }
                                break;

                            case 34:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(27, 28);
                                if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                }
                                break;

                            case 33:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 31;
                                break;

                            case 0:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                break;

                            case 1:
                                if (this.curChar == 35)
                                    this.jjCheckNAddTwoStates(6, 11);
                                break;

                            case 3:
                                if (this.curChar == 32)
                                    this.jjAddStates(108, 109);
                                break;

                            case 4:
                                if (this.curChar == 40 && kind > 12)
                                    kind = 12;
                                break;

                            case 12:
                                if ((0xffffffe7ffffffffUL & (ulong)l) == 0L)
                                    break;
                                if (kind > 21)
                                    kind = 21;
                                this.jjCheckNAdd(12);
                                break;

                            case 15:
                                if (this.curChar == 35)
                                    this.jjAddStates(106, 107);
                                break;

                            case 17:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 11)
                                    kind = 11;
                                this.jjstateSet[this.jjnewStateCnt++] = 17;
                                break;

                            case 20:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjAddStates(110, 111);
                                break;

                            case 24:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 26:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(27, 28);
                                break;

                            case 28:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 29:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(27, 28);
                                break;

                            case 30:
                                if (this.curChar == 35)
                                    this.jjCheckNAddStates(103, 105);
                                break;

                            case 31:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 32;
                                break;

                            case 32:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 22:
                                if ((0xffffffffefffffffUL & (ulong)l) != 0L)
                                {
                                    if (kind > 21)
                                        kind = 21;
                                    this.jjCheckNAdd(12);
                                }
                                else if (this.curChar == 92)
                                    this.jjCheckNAddStates(112, 115);
                                if (this.curChar == 92)
                                    this.jjAddStates(116, 117);
                                break;

                            case 14:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(25, 26);
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(23, 24);
                                if (this.curChar == 92)
                                    this.jjstateSet[this.jjnewStateCnt++] = 13;
                                break;

                            case 34:
                                if (this.curChar == 92)
                                    this.jjAddStates(116, 117);
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(25, 26);
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(23, 24);
                                break;

                            case 33:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                else if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 2:
                                if (this.curChar == 116)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 5:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 6:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 7:
                                if (this.curChar == 125)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 8:
                                if (this.curChar == 116)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                break;

                            case 9:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 8;
                                break;

                            case 10:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 9;
                                break;

                            case 11:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                break;

                            case 12:
                                if ((0xffffffffefffffffUL & (ulong)l) == 0L)
                                    break;
                                if (kind > 21)
                                    kind = 21;
                                this.jjCheckNAdd(12);
                                break;

                            case 13:
                                if (this.curChar == 92)
                                    this.jjAddStates(116, 117);
                                break;

                            case 16:
                            case 17:
                                if ((0x7fffffe87fffffeL & l) == 0L)
                                    break;
                                if (kind > 11)
                                    kind = 11;
                                this.jjCheckNAdd(17);
                                break;

                            case 18:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 19;
                                break;

                            case 19:
                            case 20:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                    this.jjCheckNAddTwoStates(20, 21);
                                break;

                            case 21:
                                if (this.curChar == 125 && kind > 11)
                                    kind = 11;
                                break;

                            case 23:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(23, 24);
                                break;

                            case 25:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(25, 26);
                                break;

                            case 27:
                                if (this.curChar == 92)
                                    this.jjAddStates(118, 119);
                                break;

                            case 32:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 22:
                            case 12:
                                if (!jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    break;
                                if (kind > 21)
                                    kind = 21;
                                this.jjCheckNAdd(12);
                                break;

                            case 32:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 34 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_7(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 2;
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_7(int pos, long active0)
        {
            return this.jjMoveNfa_7(this.jjStopStringLiteralDfa_7(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_7(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_7(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_7()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_7(0x50000L);

                case (char)(42):
                    return this.jjMoveStringLiteralDfa1_7(0x800000L);

                default:
                    return this.jjMoveNfa_7(3, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_7(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_7(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    else if ((active0 & 0x800000L) != 0L)
                        return this.jjStopAtPos(1, 23);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_7(1, 16, 0);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_7(0, active0);
        }
        private int jjMoveNfa_7(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 12;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(9, 10);
                                }
                                else if (this.curChar == 35)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 0:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 1;
                                break;

                            case 1:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            case 2:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 0;
                                break;

                            case 6:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 8:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(9, 10);
                                break;

                            case 10:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 11:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(9, 10);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(53, 56);
                                break;

                            case 1:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            case 5:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(5, 6);
                                break;

                            case 7:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(7, 8);
                                break;

                            case 9:
                                if (this.curChar == 92)
                                    this.jjAddStates(57, 58);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 1:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 12 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_8(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 2;
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_8(int pos, long active0)
        {
            return this.jjMoveNfa_8(this.jjStopStringLiteralDfa_8(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_8(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_8(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_8()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_8(0x50000L);

                default:
                    return this.jjMoveNfa_8(3, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_8(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_8(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_8(1, 16, 0);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_8(0, active0);
        }
        private int jjMoveNfa_8(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 15;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if ((0x2400L & l) != 0L)
                                {
                                    if (kind > 22)
                                        kind = 22;
                                }
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(12, 13);
                                }
                                else if (this.curChar == 35)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                if (this.curChar == 13)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 0:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 1;
                                break;

                            case 1:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            case 2:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 0;
                                break;

                            case 4:
                                if ((0x2400L & l) != 0L && kind > 22)
                                    kind = 22;
                                break;

                            case 5:
                                if (this.curChar == 10 && kind > 22)
                                    kind = 22;
                                break;

                            case 6:
                                if (this.curChar == 13)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 9:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 11:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(12, 13);
                                break;

                            case 13:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 14:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(12, 13);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 3:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(120, 123);
                                break;

                            case 1:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            case 8:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(8, 9);
                                break;

                            case 10:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(10, 11);
                                break;

                            case 12:
                                if (this.curChar == 92)
                                    this.jjAddStates(124, 125);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 1:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 15 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_4(int pos, long active0, long active1)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 27;
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        return 13;
                    }
                    return -1;

                case 1:
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 1;
                        return 13;
                    }
                    if ((active0 & 0x10000L) != 0L)
                        return 25;
                    return -1;

                case 2:
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 2;
                        return 13;
                    }
                    return -1;

                case 3:
                    if ((active0 & 0x10000000L) != 0L)
                        return 13;
                    if ((active0 & 0x20000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 3;
                        return 13;
                    }
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_4(int pos, long active0, long active1)
        {
            return this.jjMoveNfa_4(this.jjStopStringLiteralDfa_4(pos, active0, active1), pos + 1);
        }
        private int jjStartNfaWithStates_4(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_4(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_4()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_4(0x50000L);

                case (char)(102):
                    return this.jjMoveStringLiteralDfa1_4(0x20000000L);

                case (char)(116):
                    return this.jjMoveStringLiteralDfa1_4(0x10000000L);

                case (char)(123):
                    return this.jjStopAtPos(0, 64);

                case (char)(125):
                    return this.jjStopAtPos(0, 65);

                default:
                    return this.jjMoveNfa_4(12, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_4(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_4(0, active0, 0L);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_4(1, 16, 25);
                    break;

                case (char)(97):
                    return this.jjMoveStringLiteralDfa2_4(active0, 0x20000000L);

                case (char)(114):
                    return this.jjMoveStringLiteralDfa2_4(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_4(0, active0, 0L);
        }
        private int jjMoveStringLiteralDfa2_4(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_4(0, old0, 0L);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_4(1, active0, 0L);
                return 2;
            }
            switch (this.curChar)
            {

                case (char)(108):
                    return this.jjMoveStringLiteralDfa3_4(active0, 0x20000000L);

                case (char)(117):
                    return this.jjMoveStringLiteralDfa3_4(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_4(1, active0, 0L);
        }
        private int jjMoveStringLiteralDfa3_4(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_4(1, old0, 0L);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_4(2, active0, 0L);
                return 3;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x10000000L) != 0L)
                        return this.jjStartNfaWithStates_4(3, 28, 13);
                    break;

                case (char)(115):
                    return this.jjMoveStringLiteralDfa4_4(active0, 0x20000000L);

                default:
                    break;

            }
            return this.jjStartNfa_4(2, active0, 0L);
        }
        private int jjMoveStringLiteralDfa4_4(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_4(2, old0, 0L);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_4(3, active0, 0L);
                return 4;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x20000000L) != 0L)
                        return this.jjStartNfaWithStates_4(4, 29, 13);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_4(3, active0, 0L);
        }
        private int jjMoveNfa_4(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 28;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 12:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                else if (this.curChar == 35)
                                    this.jjCheckNAddStates(126, 128);
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(21, 22);
                                }
                                else if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 15;
                                break;

                            case 27:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 25;
                                break;

                            case 0:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                break;

                            case 1:
                                if (this.curChar == 35)
                                    this.jjCheckNAddTwoStates(6, 11);
                                break;

                            case 3:
                                if (this.curChar == 32)
                                    this.jjAddStates(108, 109);
                                break;

                            case 4:
                                if (this.curChar == 40 && kind > 12)
                                    kind = 12;
                                break;

                            case 13:
                                if ((0x3ff200000000000L & l) == 0L)
                                    break;
                                if (kind > 62)
                                    kind = 62;
                                this.jjstateSet[this.jjnewStateCnt++] = 13;
                                break;

                            case 14:
                                if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 15;
                                break;

                            case 18:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 20:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(21, 22);
                                break;

                            case 22:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 23:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(21, 22);
                                break;

                            case 24:
                                if (this.curChar == 35)
                                    this.jjCheckNAddStates(126, 128);
                                break;

                            case 25:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 26;
                                break;

                            case 26:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 12:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                {
                                    if (kind > 62)
                                        kind = 62;
                                    this.jjCheckNAdd(13);
                                }
                                else if (this.curChar == 92)
                                    this.jjCheckNAddStates(129, 132);
                                break;

                            case 27:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                else if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 2:
                                if (this.curChar == 116)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 5:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 6:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 7:
                                if (this.curChar == 125)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 8:
                                if (this.curChar == 116)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                break;

                            case 9:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 8;
                                break;

                            case 10:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 9;
                                break;

                            case 11:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                break;

                            case 13:
                                if ((0x7fffffe87fffffeL & l) == 0L)
                                    break;
                                if (kind > 62)
                                    kind = 62;
                                this.jjCheckNAdd(13);
                                break;

                            case 15:
                                if ((0x7fffffe07fffffeL & l) != 0L && kind > 63)
                                    kind = 63;
                                break;

                            case 16:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(129, 132);
                                break;

                            case 17:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(17, 18);
                                break;

                            case 19:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(19, 20);
                                break;

                            case 21:
                                if (this.curChar == 92)
                                    this.jjAddStates(133, 134);
                                break;

                            case 26:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 26:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 28 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_1(int pos, long active0)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 48;
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        return 36;
                    }
                    if ((active0 & 0x10L) != 0L)
                        return 70;
                    return -1;

                case 1:
                    if ((active0 & 0x10000L) != 0L)
                        return 46;
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 1;
                        return 36;
                    }
                    return -1;

                case 2:
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 2;
                        return 36;
                    }
                    return -1;

                case 3:
                    if ((active0 & 0x10000000L) != 0L)
                        return 36;
                    if ((active0 & 0x20000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 3;
                        return 36;
                    }
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_1(int pos, long active0)
        {
            return this.jjMoveNfa_1(this.jjStopStringLiteralDfa_1(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_1(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_1(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_1()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_1(0x50000L);

                case (char)(41):
                    return this.jjStopAtPos(0, 10);

                case (char)(44):
                    return this.jjStopAtPos(0, 3);

                case (char)(46):
                    return this.jjMoveStringLiteralDfa1_1(0x10L);

                case (char)(58):
                    return this.jjStopAtPos(0, 5);

                case (char)(91):
                    return this.jjStopAtPos(0, 1);

                case (char)(93):
                    return this.jjStopAtPos(0, 2);

                case (char)(102):
                    return this.jjMoveStringLiteralDfa1_1(0x20000000L);

                case (char)(116):
                    return this.jjMoveStringLiteralDfa1_1(0x10000000L);

                case (char)(123):
                    return this.jjStopAtPos(0, 6);

                case (char)(125):
                    return this.jjStopAtPos(0, 7);

                default:
                    return this.jjMoveNfa_1(13, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_1(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_1(0, active0);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_1(1, 16, 46);
                    break;

                case (char)(46):
                    if ((active0 & 0x10L) != 0L)
                        return this.jjStopAtPos(1, 4);
                    break;

                case (char)(97):
                    return this.jjMoveStringLiteralDfa2_1(active0, 0x20000000L);

                case (char)(114):
                    return this.jjMoveStringLiteralDfa2_1(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_1(0, active0);
        }
        private int jjMoveStringLiteralDfa2_1(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_1(0, old0);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_1(1, active0);
                return 2;
            }
            switch (this.curChar)
            {

                case (char)(108):
                    return this.jjMoveStringLiteralDfa3_1(active0, 0x20000000L);

                case (char)(117):
                    return this.jjMoveStringLiteralDfa3_1(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_1(1, active0);
        }
        private int jjMoveStringLiteralDfa3_1(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_1(1, old0);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_1(2, active0);
                return 3;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x10000000L) != 0L)
                        return this.jjStartNfaWithStates_1(3, 28, 36);
                    break;

                case (char)(115):
                    return this.jjMoveStringLiteralDfa4_1(active0, 0x20000000L);

                default:
                    break;

            }
            return this.jjStartNfa_1(2, active0);
        }
        private int jjMoveStringLiteralDfa4_1(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_1(2, old0);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_1(3, active0);
                return 4;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x20000000L) != 0L)
                        return this.jjStartNfaWithStates_1(4, 29, 36);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_1(3, active0);
        }
        private int jjMoveNfa_1(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 71;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 13:
                                if ((0x3ff000000000000L & l) != 0L)
                                {
                                    if (kind > 52)
                                        kind = 52;
                                    this.jjCheckNAddStates(135, 140);
                                }
                                else if ((0x100002600L & l) != 0L)
                                {
                                    if (kind > 26)
                                        kind = 26;
                                    this.jjCheckNAdd(12);
                                }
                                else if (this.curChar == 46)
                                    this.jjCheckNAddTwoStates(60, 70);
                                else if (this.curChar == 45)
                                    this.jjCheckNAddStates(141, 144);
                                else if (this.curChar == 35)
                                    this.jjCheckNAddStates(145, 147);
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(42, 43);
                                }
                                else if (this.curChar == 39)
                                    this.jjCheckNAddStates(148, 150);
                                else if (this.curChar == 34)
                                    this.jjCheckNAddStates(151, 153);
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                break;

                            case 70:
                            case 60:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(60, 61);
                                break;

                            case 48:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 46;
                                break;

                            case 0:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                break;

                            case 1:
                                if (this.curChar == 35)
                                    this.jjCheckNAddTwoStates(6, 11);
                                break;

                            case 3:
                                if (this.curChar == 32)
                                    this.jjAddStates(108, 109);
                                break;

                            case 4:
                                if (this.curChar == 40 && kind > 12)
                                    kind = 12;
                                break;

                            case 12:
                                if ((0x100002600L & l) == 0L)
                                    break;
                                if (kind > 26)
                                    kind = 26;
                                this.jjCheckNAdd(12);
                                break;

                            case 14:
                                if ((0xfffffffbffffffffUL & (ulong)l) != 0L)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 15:
                                if (this.curChar == 34 && kind > 27)
                                    kind = 27;
                                break;

                            case 17:
                                if ((0x8400000000L & l) != 0L)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 18:
                                if ((0xff000000000000L & l) != 0L)
                                    this.jjCheckNAddStates(154, 157);
                                break;

                            case 19:
                                if ((0xff000000000000L & l) != 0L)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 20:
                                if ((0xf000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 21;
                                break;

                            case 21:
                                if ((0xff000000000000L & l) != 0L)
                                    this.jjCheckNAdd(19);
                                break;

                            case 23:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 24;
                                break;

                            case 24:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 25;
                                break;

                            case 25:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 26;
                                break;

                            case 26:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 27:
                                if (this.curChar == 32)
                                    this.jjAddStates(118, 119);
                                break;

                            case 28:
                                if (this.curChar == 10)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 29:
                                if (this.curChar == 39)
                                    this.jjCheckNAddStates(148, 150);
                                break;

                            case 30:

                                if ((0xffffff7fffffffffUL & (ulong)l) != 0L)
                                    this.jjCheckNAddStates(148, 150);
                                break;

                            case 32:
                                if (this.curChar == 32)
                                    this.jjAddStates(158, 159);
                                break;

                            case 33:
                                if (this.curChar == 10)
                                    this.jjCheckNAddStates(148, 150);
                                break;

                            case 34:
                                if (this.curChar == 39 && kind > 27)
                                    kind = 27;
                                break;

                            case 36:
                                if ((0x3ff200000000000L & l) == 0L)
                                    break;
                                if (kind > 62)
                                    kind = 62;
                                this.jjstateSet[this.jjnewStateCnt++] = 36;
                                break;

                            case 39:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 41:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(42, 43);
                                break;

                            case 43:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 44:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(42, 43);
                                break;

                            case 45:
                                if (this.curChar == 35)
                                    this.jjCheckNAddStates(145, 147);
                                break;

                            case 46:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 47;
                                break;

                            case 47:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            case 49:
                                if (this.curChar == 45)
                                    this.jjCheckNAddStates(141, 144);
                                break;

                            case 50:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 52)
                                    kind = 52;
                                this.jjCheckNAddTwoStates(50, 52);
                                break;

                            case 51:
                                if (this.curChar == 46 && kind > 52)
                                    kind = 52;
                                break;

                            case 52:
                                if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 51;
                                break;

                            case 53:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(53, 54);
                                break;

                            case 54:
                                if (this.curChar != 46)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(55, 56);
                                break;

                            case 55:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAddTwoStates(55, 56);
                                break;

                            case 57:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(58);
                                break;

                            case 58:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(58);
                                break;

                            case 59:
                                if (this.curChar == 46)
                                    this.jjCheckNAdd(60);
                                break;

                            case 62:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(63);
                                break;

                            case 63:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(63);
                                break;

                            case 64:
                                if ((0x3ff000000000000L & l) != 0L)
                                    this.jjCheckNAddTwoStates(64, 65);
                                break;

                            case 66:
                                if ((0x280000000000L & l) != 0L)
                                    this.jjCheckNAdd(67);
                                break;

                            case 67:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 53)
                                    kind = 53;
                                this.jjCheckNAdd(67);
                                break;

                            case 68:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 52)
                                    kind = 52;
                                this.jjCheckNAddStates(135, 140);
                                break;

                            case 69:
                                if (this.curChar == 46)
                                    this.jjCheckNAddTwoStates(60, 70);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 13:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                {
                                    if (kind > 62)
                                        kind = 62;
                                    this.jjCheckNAdd(36);
                                }
                                else if (this.curChar == 92)
                                    this.jjCheckNAddStates(160, 163);
                                break;

                            case 70:
                                if ((0x7fffffe07fffffeL & l) != 0L && kind > 63)
                                    kind = 63;
                                break;

                            case 48:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                else if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 2:
                                if (this.curChar == 116)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 5:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 6:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 7:
                                if (this.curChar == 125)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 8:
                                if (this.curChar == 116)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                break;

                            case 9:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 8;
                                break;

                            case 10:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 9;
                                break;

                            case 11:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                break;

                            case 14:
                                this.jjCheckNAddStates(151, 153);
                                break;

                            case 16:
                                if (this.curChar == 92)
                                    this.jjAddStates(164, 169);
                                break;

                            case 17:
                                if ((0x14404410000000L & l) != 0L)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 22:
                                if (this.curChar == 117)
                                    this.jjstateSet[this.jjnewStateCnt++] = 23;
                                break;

                            case 23:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 24;
                                break;

                            case 24:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 25;
                                break;

                            case 25:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjstateSet[this.jjnewStateCnt++] = 26;
                                break;

                            case 26:
                                if ((0x7e0000007eL & l) != 0L)
                                    this.jjCheckNAddStates(151, 153);
                                break;

                            case 30:
                                this.jjAddStates(148, 150);
                                break;

                            case 31:
                                if (this.curChar == 92)
                                    this.jjAddStates(158, 159);
                                break;

                            case 35:
                            case 36:
                                if ((0x7fffffe87fffffeL & l) == 0L)
                                    break;
                                if (kind > 62)
                                    kind = 62;
                                this.jjCheckNAdd(36);
                                break;

                            case 37:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(160, 163);
                                break;

                            case 38:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(38, 39);
                                break;

                            case 40:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(40, 41);
                                break;

                            case 42:
                                if (this.curChar == 92)
                                    this.jjAddStates(170, 171);
                                break;

                            case 47:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            case 56:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(172, 173);
                                break;

                            case 61:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(174, 175);
                                break;

                            case 65:
                                if ((0x2000000020L & l) != 0L)
                                    this.jjAddStates(27, 28);
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 14:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    this.jjAddStates(151, 153);
                                break;

                            case 30:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    this.jjAddStates(148, 150);
                                break;

                            case 47:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 71 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_2(int pos, long active0, long active1)
        {
            switch (pos)
            {

                case 0:
                    if ((active0 & 0x70000L) != 0L)
                        return 27;
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        return 13;
                    }
                    return -1;

                case 1:
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 1;
                        return 13;
                    }
                    if ((active0 & 0x10000L) != 0L)
                        return 25;
                    return -1;

                case 2:
                    if ((active0 & 0x30000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 2;
                        return 13;
                    }
                    return -1;

                case 3:
                    if ((active0 & 0x10000000L) != 0L)
                        return 13;
                    if ((active0 & 0x20000000L) != 0L)
                    {
                        this.jjmatchedKind = 62;
                        this.jjmatchedPos = 3;
                        return 13;
                    }
                    return -1;

                default:
                    return -1;

            }
        }
        private int jjStartNfa_2(int pos, long active0, long active1)
        {
            return this.jjMoveNfa_2(this.jjStopStringLiteralDfa_2(pos, active0, active1), pos + 1);
        }
        private int jjStartNfaWithStates_2(int pos, int kind, int state)
        {
            this.jjmatchedKind = kind;
            this.jjmatchedPos = pos;
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return this.jjMoveNfa_2(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_2()
        {
            switch (this.curChar)
            {

                case (char)(35):
                    this.jjmatchedKind = 17;
                    return this.jjMoveStringLiteralDfa1_2(0x50000L);

                case (char)(40):
                    return this.jjStopAtPos(0, 8);

                case (char)(102):
                    return this.jjMoveStringLiteralDfa1_2(0x20000000L);

                case (char)(116):
                    return this.jjMoveStringLiteralDfa1_2(0x10000000L);

                case (char)(123):
                    return this.jjStopAtPos(0, 64);

                case (char)(125):
                    return this.jjStopAtPos(0, 65);

                default:
                    return this.jjMoveNfa_2(12, 0);

            }
        }
        private int jjMoveStringLiteralDfa1_2(long active0)
        {
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_2(0, active0, 0L);
                return 1;
            }
            switch (this.curChar)
            {

                case (char)(35):
                    if ((active0 & 0x40000L) != 0L)
                        return this.jjStopAtPos(1, 18);
                    break;

                case (char)(42):
                    if ((active0 & 0x10000L) != 0L)
                        return this.jjStartNfaWithStates_2(1, 16, 25);
                    break;

                case (char)(97):
                    return this.jjMoveStringLiteralDfa2_2(active0, 0x20000000L);

                case (char)(114):
                    return this.jjMoveStringLiteralDfa2_2(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_2(0, active0, 0L);
        }
        private int jjMoveStringLiteralDfa2_2(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_2(0, old0, 0L);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_2(1, active0, 0L);
                return 2;
            }
            switch (this.curChar)
            {

                case (char)(108):
                    return this.jjMoveStringLiteralDfa3_2(active0, 0x20000000L);

                case (char)(117):
                    return this.jjMoveStringLiteralDfa3_2(active0, 0x10000000L);

                default:
                    break;

            }
            return this.jjStartNfa_2(1, active0, 0L);
        }
        private int jjMoveStringLiteralDfa3_2(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_2(1, old0, 0L);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_2(2, active0, 0L);
                return 3;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x10000000L) != 0L)
                        return this.jjStartNfaWithStates_2(3, 28, 13);
                    break;

                case (char)(115):
                    return this.jjMoveStringLiteralDfa4_2(active0, 0x20000000L);

                default:
                    break;

            }
            return this.jjStartNfa_2(2, active0, 0L);
        }
        private int jjMoveStringLiteralDfa4_2(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return this.jjStartNfa_2(2, old0, 0L);
            try
            {
                this.curChar = this.input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                this.jjStopStringLiteralDfa_2(3, active0, 0L);
                return 4;
            }
            switch (this.curChar)
            {

                case (char)(101):
                    if ((active0 & 0x20000000L) != 0L)
                        return this.jjStartNfaWithStates_2(4, 29, 13);
                    break;

                default:
                    break;

            }
            return this.jjStartNfa_2(3, active0, 0L);
        }
        private int jjMoveNfa_2(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            this.jjnewStateCnt = 28;
            int i = 1;
            this.jjstateSet[0] = (uint)startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++this.jjround == 0x7fffffff)
                    this.ReInitRounds();
                if (this.curChar < 64)
                {
                    long l = 1L << (int)this.curChar;

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 12:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                else if (this.curChar == 35)
                                    this.jjCheckNAddStates(126, 128);
                                else if (this.curChar == 36)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    this.jjCheckNAddTwoStates(21, 22);
                                }
                                else if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 15;
                                break;

                            case 27:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 25;
                                break;

                            case 0:
                                if ((0x100000200L & l) != 0L)
                                    this.jjCheckNAddTwoStates(0, 1);
                                break;

                            case 1:
                                if (this.curChar == 35)
                                    this.jjCheckNAddTwoStates(6, 11);
                                break;

                            case 3:
                                if (this.curChar == 32)
                                    this.jjAddStates(108, 109);
                                break;

                            case 4:
                                if (this.curChar == 40 && kind > 12)
                                    kind = 12;
                                break;

                            case 13:
                                if ((0x3ff200000000000L & l) == 0L)
                                    break;
                                if (kind > 62)
                                    kind = 62;
                                this.jjstateSet[this.jjnewStateCnt++] = 13;
                                break;

                            case 14:
                                if (this.curChar == 46)
                                    this.jjstateSet[this.jjnewStateCnt++] = 15;
                                break;

                            case 18:
                                if (this.curChar == 36 && kind > 13)
                                    kind = 13;
                                break;

                            case 20:
                                if (this.curChar == 36)
                                    this.jjCheckNAddTwoStates(21, 22);
                                break;

                            case 22:
                                if (this.curChar == 33 && kind > 14)
                                    kind = 14;
                                break;

                            case 23:
                                if (this.curChar != 36)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                this.jjCheckNAddTwoStates(21, 22);
                                break;

                            case 24:
                                if (this.curChar == 35)
                                    this.jjCheckNAddStates(126, 128);
                                break;

                            case 25:
                                if (this.curChar == 42)
                                    this.jjstateSet[this.jjnewStateCnt++] = 26;
                                break;

                            case 26:
                                if ((0xfffffff7ffffffffUL & (ulong)l) != 0L && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else if (this.curChar < 128)
                {
                    long l = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 12:
                                if ((0x7fffffe87fffffeL & l) != 0L)
                                {
                                    if (kind > 62)
                                        kind = 62;
                                    this.jjCheckNAdd(13);
                                }
                                else if (this.curChar == 92)
                                    this.jjCheckNAddStates(129, 132);
                                break;

                            case 27:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                else if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 2:
                                if (this.curChar == 116)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 5:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 2;
                                break;

                            case 6:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 5;
                                break;

                            case 7:
                                if (this.curChar == 125)
                                    this.jjCheckNAddTwoStates(3, 4);
                                break;

                            case 8:
                                if (this.curChar == 116)
                                    this.jjstateSet[this.jjnewStateCnt++] = 7;
                                break;

                            case 9:
                                if (this.curChar == 101)
                                    this.jjstateSet[this.jjnewStateCnt++] = 8;
                                break;

                            case 10:
                                if (this.curChar == 115)
                                    this.jjstateSet[this.jjnewStateCnt++] = 9;
                                break;

                            case 11:
                                if (this.curChar == 123)
                                    this.jjstateSet[this.jjnewStateCnt++] = 10;
                                break;

                            case 13:
                                if ((0x7fffffe87fffffeL & l) == 0L)
                                    break;
                                if (kind > 62)
                                    kind = 62;
                                this.jjCheckNAdd(13);
                                break;

                            case 15:
                                if ((0x7fffffe07fffffeL & l) != 0L && kind > 63)
                                    kind = 63;
                                break;

                            case 16:
                                if (this.curChar == 92)
                                    this.jjCheckNAddStates(129, 132);
                                break;

                            case 17:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(17, 18);
                                break;

                            case 19:
                                if (this.curChar == 92)
                                    this.jjCheckNAddTwoStates(19, 20);
                                break;

                            case 21:
                                if (this.curChar == 92)
                                    this.jjAddStates(133, 134);
                                break;

                            case 26:
                                if (kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int hiByte = (int)(this.curChar >> 8);
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 63);
                    int i2 = (this.curChar & 0xff) >> 6;
                    long l2 = 1L << (this.curChar & 63);

                MatchLoop1:
                    do
                    {
                        switch (this.jjstateSet[--i])
                        {

                            case 26:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 15)
                                    kind = 15;
                                break;

                            default: break;

                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    this.jjmatchedKind = kind;
                    this.jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = this.jjnewStateCnt) == (startsAt = 28 - (this.jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    this.curChar = this.input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        //UPGRADE_NOTE: Final �Ѵӡ�jjnextStates�����������Ƴ��� "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] jjnextStates = new int[] { 87, 89, 90, 91, 96, 97, 87, 90, 57, 96, 27, 28, 31, 11, 12, 13, 1, 2, 4, 11, 16, 12, 13, 24, 25, 29, 30, 66, 67, 69, 70, 71, 72, 83, 85, 80, 81, 77, 78, 14, 15, 17, 19, 24, 25, 60, 61, 73, 74, 94, 95, 98, 99, 5, 6, 7, 8, 9, 10, 78, 80, 81, 82, 87, 88, 78, 81, 10, 87, 19, 20, 31, 32, 34, 42, 43, 45, 50, 32, 51, 66, 43, 67, 54, 57, 64, 71, 76, 22, 23, 24, 25, 35, 40, 47, 13, 14, 26, 27, 85, 86, 89, 90, 6, 11, 33, 16, 18, 3, 4, 20, 21, 23, 24, 25, 26, 14, 15, 27, 28, 8, 9, 10, 11, 12, 13, 6, 11, 27, 17, 18, 19, 20, 21, 22, 50, 52, 53, 54, 64, 65, 50, 53, 59, 64, 6, 11, 48, 30, 31, 34, 14, 15, 16, 14, 19, 15, 16, 32, 33, 38, 39, 40, 41, 17, 18, 20, 22, 27, 28, 42, 43, 57, 58, 62, 63 };
        private static bool jjCanMove_0(int hiByte, int i1, int i2, long l1, long l2)
        {
            switch (hiByte)
            {

                case 0:
                    return ((jjbitVec2[i2] & (ulong)l2) != 0L);

                default:
                    if ((jjbitVec0[i1] & (ulong)l1) != 0L)
                        return true;
                    return false;

            }
        }
        public static readonly string[] jjstrLiteralImages = { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
        public static readonly string[] lexStateNames = { "DIRECTIVE", "REFMOD2", "REFMODIFIER", "DEFAULT", "REFERENCE", "PRE_DIRECTIVE", "IN_MULTI_LINE_COMMENT", "IN_FORMAL_COMMENT", "IN_SINGLE_LINE_COMMENT" };
        public static readonly int[] jjnewLexState = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        internal static readonly ulong[] jjtoToken = { 0xc637fffffdfc1fffL, 0x3L };
        internal static readonly long[] jjtoSkip = { 0x2000000L, 0xcL };
        internal static readonly long[] jjtoSpecial = { 0x0L, 0xcL };
        internal static readonly long[] jjtoMore = { 0x3e000L, 0x0L };
        protected internal ICharStream input_stream;
        private uint[] jjrounds = new uint[101];
        private uint[] jjstateSet = new uint[202];
        internal System.Text.StringBuilder image;
        internal int jjimageLen;
        internal int lengthOfMatch;
        protected internal char curChar;
        public ParserTokenManager(ICharStream stream)
        {
            this.InitBlock();
            this.input_stream = stream;
        }
        public ParserTokenManager(ICharStream stream, int lexState)
            : this(stream)
        {
            this.SwitchTo(lexState);
        }
        public virtual void ReInit(ICharStream stream)
        {
            this.jjmatchedPos = this.jjnewStateCnt = 0;
            this.curLexState = this.defaultLexState;
            this.input_stream = stream;
            this.ReInitRounds();
        }
        private void ReInitRounds()
        {
            int i;
            this.jjround = 0x80000001;
            for (i = 101; i-- > 0; )
                this.jjrounds[i] = 0x80000000;
        }
        public virtual void ReInit(ICharStream stream, int lexState)
        {
            this.ReInit(stream);
            this.SwitchTo(lexState);
        }
        public virtual void SwitchTo(int lexState)
        {
            if (lexState >= 9 || lexState < 0)
                throw new TokenMgrError("Error: Ignoring invalid lexical state : " + lexState + ". State unchanged.", TokenMgrError.INVALID_LEXICAL_STATE);
            else
                this.curLexState = lexState;
        }

        protected internal virtual Token jjFillToken()
        {
            Token t = Token.NewToken(this.jjmatchedKind);
            t.Kind = this.jjmatchedKind;
            System.String im = jjstrLiteralImages[this.jjmatchedKind];
            t.Image = (im == null) ? this.input_stream.GetImage() : im;
            t.BeginLine = this.input_stream.BeginLine;
            t.BeginColumn = this.input_stream.BeginColumn;
            t.EndLine = this.input_stream.EndLine;
            t.EndColumn = this.input_stream.EndColumn;
            return t;
        }

        internal int curLexState = 3;
        internal int defaultLexState = 3;
        internal int jjnewStateCnt;
        internal uint jjround;
        internal int jjmatchedPos;
        internal int jjmatchedKind;

        internal virtual void SkipLexicalActions(Token matchedToken)
        {
            switch (this.jjmatchedKind)
            {

                case 66:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    /*
                    * push every terminator character back into the stream
                    */

                    this.input_stream.Backup(1);

                    this.inReference = false;

                    if (this.debugPrint)
                        System.Console.Out.Write("REF_TERM :");

                    this.stateStackPop();
                    break;

                case 67:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    if (this.debugPrint)
                        System.Console.Out.Write("DIRECTIVE_TERM :");

                    this.input_stream.Backup(1);
                    this.inDirective = false;
                    this.stateStackPop();
                    break;

                default:
                    break;

            }
        }
        internal virtual void MoreLexicalActions()
        {
            this.jjimageLen += (this.lengthOfMatch = this.jjmatchedPos + 1);
            switch (this.jjmatchedKind)
            {

                case 13:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen)));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen));
                    this.jjimageLen = 0;
                    if (!this.inComment)
                    {
                        /*
                        * if we find ourselves in REFERENCE, we need to pop down
                        * to end the previous ref
                        */

                        if (this.curLexState == ParserConstants.REFERENCE)
                        {
                            this.inReference = false;
                            this.stateStackPop();
                        }

                        this.inReference = true;

                        if (this.debugPrint)
                            System.Console.Out.Write("$  : going to " + ParserConstants.REFERENCE);

                        this.stateStackPush();
                        this.SwitchTo(ParserConstants.REFERENCE);
                    }
                    break;

                case 14:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen)));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen));
                    this.jjimageLen = 0;
                    if (!this.inComment)
                    {
                        /*
                        * if we find ourselves in REFERENCE, we need to pop down
                        * to end the previous ref
                        */

                        if (this.curLexState == ParserConstants.REFERENCE)
                        {
                            this.inReference = false;
                            this.stateStackPop();
                        }

                        this.inReference = true;

                        if (this.debugPrint)
                            System.Console.Out.Write("$!  : going to " + ParserConstants.REFERENCE);

                        this.stateStackPush();
                        this.SwitchTo(ParserConstants.REFERENCE);
                    }
                    break;

                case 15:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen)));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen));
                    this.jjimageLen = 0;
                    if (!this.inComment)
                    {
                        this.input_stream.Backup(1);
                        this.inComment = true;
                        this.stateStackPush();
                        this.SwitchTo(ParserConstants.IN_FORMAL_COMMENT);
                    }
                    break;

                case 16:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen)));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen));
                    this.jjimageLen = 0;
                    if (!this.inComment)
                    {
                        this.inComment = true;
                        this.stateStackPush();
                        this.SwitchTo(ParserConstants.IN_MULTI_LINE_COMMENT);
                    }
                    break;

                case 17:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen)));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen));
                    this.jjimageLen = 0;
                    if (!this.inComment)
                    {
                        /*
                        * We can have the situation where #if($foo)$foo#end.
                        * We need to transition out of REFERENCE before going to DIRECTIVE.
                        * I don't really like this, but I can't think of a legal way
                        * you are going into DIRECTIVE while in REFERENCE.  -gmj
                        */

                        if (this.curLexState == ParserConstants.REFERENCE || this.curLexState == ParserConstants.REFMODIFIER)
                        {
                            this.inReference = false;
                            this.stateStackPop();
                        }

                        this.inDirective = true;

                        if (this.debugPrint)
                            System.Console.Out.Write("# :  going to " + ParserConstants.DIRECTIVE);

                        this.stateStackPush();
                        this.SwitchTo(ParserConstants.PRE_DIRECTIVE);
                    }
                    break;

                default:
                    break;

            }
        }
        internal virtual void TokenLexicalActions(Token matchedToken)
        {
            switch (this.jjmatchedKind)
            {

                case 8:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    if (!this.inComment)
                        this.lparen++;

                    /*
                    * If in REFERENCE and we have seen the dot, then move
                    * to REFMOD2 -> Modifier()
                    */

                    if (this.curLexState == ParserConstants.REFMODIFIER)
                        this.SwitchTo(ParserConstants.REFMOD2);
                    break;

                case 9:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.RPARENHandler();
                    break;

                case 10:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    /*
                    * need to simply switch back to REFERENCE, not drop down the stack
                    * because we can (infinitely) chain, ala
                    * $foo.bar().blargh().woogie().doogie()
                    */

                    this.SwitchTo(ParserConstants.REFERENCE);
                    break;

                case 12:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    if (!this.inComment)
                    {
                        this.inDirective = true;

                        if (this.debugPrint)
                            System.Console.Out.Write("#set :  going to " + ParserConstants.DIRECTIVE);

                        this.stateStackPush();
                        this.inSet = true;
                        this.SwitchTo(ParserConstants.DIRECTIVE);
                    }

                    /*
                    *  need the LPAREN action
                    */

                    if (!this.inComment)
                    {
                        this.lparen++;

                        /*
                        * If in REFERENCE and we have seen the dot, then move
                        * to REFMOD2 -> Modifier()
                        */

                        if (this.curLexState == ParserConstants.REFMODIFIER)
                            this.SwitchTo(ParserConstants.REFMOD2);
                    }
                    break;

                case 18:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    if (!this.inComment)
                    {
                        if (this.curLexState == ParserConstants.REFERENCE)
                        {
                            this.inReference = false;
                            this.stateStackPop();
                        }

                        this.inComment = true;
                        this.stateStackPush();
                        this.SwitchTo(ParserConstants.IN_SINGLE_LINE_COMMENT);
                    }
                    break;

                case 22:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.inComment = false;
                    this.stateStackPop();
                    break;

                case 23:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.inComment = false;
                    this.stateStackPop();
                    break;

                case 24:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.inComment = false;
                    this.stateStackPop();
                    break;

                case 27:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    /*
                    *  - if we are in DIRECTIVE and haven't seen ( yet, then also drop out.
                    *      don't forget to account for the beloved yet wierd #set
                    *  - finally, if we are in REFMOD2 (remember : $foo.bar( ) then " is ok!
                    */

                    if (this.curLexState == ParserConstants.DIRECTIVE && !this.inSet && this.lparen == 0)
                        this.stateStackPop();
                    break;

                case 30:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    if (this.debugPrint)
                        System.Console.Out.WriteLine(" NEWLINE :");

                    this.stateStackPop();

                    if (this.inSet)
                        this.inSet = false;

                    if (this.inDirective)
                        this.inDirective = false;
                    break;

                case 46:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.inDirective = false;
                    this.stateStackPop();
                    break;

                case 47:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.SwitchTo(ParserConstants.DIRECTIVE);
                    break;

                case 48:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.SwitchTo(ParserConstants.DIRECTIVE);
                    break;

                case 49:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.inDirective = false;
                    this.stateStackPop();
                    break;

                case 50:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.inDirective = false;
                    this.stateStackPop();
                    break;

                case 52:
                    if (this.image == null)
                        this.image = new StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    /*
                    * Remove the double period if it is there
                    */
                    if (matchedToken.Image.EndsWith(".."))
                    {
                        this.input_stream.Backup(2);
                        matchedToken.Image = matchedToken.Image.Substring(0, (matchedToken.Image.Length - 2) - (0));
                    }

                    /*
                    * check to see if we are in set
                    *    ex.  #set $foo = $foo + 3
                    *  because we want to handle the \n after
                    */

                    if (this.lparen == 0 && !this.inSet && this.curLexState != ParserConstants.REFMOD2)
                    {
                        this.stateStackPop();
                    }
                    break;

                case 53:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    /*
                    * check to see if we are in set
                    *    ex.  #set $foo = $foo + 3
                    *  because we want to handle the \n after
                    */

                    if (this.lparen == 0 && !this.inSet && this.curLexState != ParserConstants.REFMOD2)
                    {
                        this.stateStackPop();
                    }
                    break;

                case 63:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    /*
                    * push the alpha char back into the stream so the following identifier
                    * is complete
                    */

                    this.input_stream.Backup(1);

                    /*
                    * and munge the <DOT> so we just get a . when we have normal text that
                    * looks like a ref.ident
                    */

                    matchedToken.Image = ".";

                    if (this.debugPrint)
                        System.Console.Out.Write("DOT : switching to " + ParserConstants.REFMODIFIER);
                    this.SwitchTo(ParserConstants.REFMODIFIER);
                    break;

                case 65:
                    if (this.image == null)
                        this.image = new System.Text.StringBuilder(new System.String(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1))));
                    else
                        this.image.Append(this.input_stream.GetSuffix(this.jjimageLen + (this.lengthOfMatch = this.jjmatchedPos + 1)));
                    this.stateStackPop();
                    break;

                default:
                    break;

            }
        }
    }
}